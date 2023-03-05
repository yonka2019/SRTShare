﻿using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using CConsole = SRTShareLib.CColorManager;  // Colored Console
using Control = SRTShareLib.SRTManager.ProtocolFields.Control;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    public partial class MainView : Form
    {
        private readonly Random rnd = new Random();

        private bool serverAlive = false;  // for icmp request - answer check
        private bool handledArp = false;  // to avoid secondly induction to server (only for LOOPBACK connections (same pc server/client))

        private bool videoStage = false;  // when the client reaches the video stage, he knows that each packet from the server will be encrypted,
                                          // so he should be ready to decrypt each received packet

        internal static uint server_sid = 0;  // we getting know this value on the induction that the server returns to us (SID -> Socket ID)
        internal static string server_mac = null;

        internal static ushort my_client_port;
        internal static uint my_client_sid = 0;  // the server sends this value on the induction answer (SID -> Socket ID) (MY SID)

        internal static bool externalConnection;
        internal static bool AutoQualityControl = false;

        private static Thread handlePackets, handleKeepAlive;

        // 10% - q_10p (button)
        // 20% - q_10p (button)
        //  .. .. ..
        internal static Dictionary<long, ToolStripMenuItem> QualityButtons;
        internal static BaseEncryption Server_EncryptionControl;


#if DEBUG
        private static ulong dataReceived = 0;  // count data packets received (included chunks)
#endif

        //  - CONVERSATION SETTINGS - + - + - + - + - + - + - + - +

        internal const EncryptionType ENCRYPTION = EncryptionType.AES256;  // The whole encryption of the conversation (from data stage)
        internal const int INITIAL_PSN = 1;  // The first sequence number of the conversation  ! [ MUST NOT BE 0 ] !

        internal const int DATA_LOSS_PERCENT_REQUIRED = 3;  // loss percent which is required in order to send decrease quality update request to the server
        internal const int DATA_DECREASE_QUALITY_BY = 10; // (0 - 100)
        internal const bool AUTO_QUALITY_CONTROL = false;
        internal const bool RETRANSMISSION_MODE = true;
        // DEFAULT QUALITY VALUE (to server and client) - ProtocolManager.cs : DEFAULT_QUALITY

        //  - CONVERSATION SETTINGS - + - + - + - + - + - + - + - +

        public MainView()
        {
            InitializeComponent();

            autoQualityControl.Checked = AUTO_QUALITY_CONTROL;
            AutoQualityControl = autoQualityControl.Checked;

            QualityButtons = new Dictionary<long, ToolStripMenuItem> { { 10L, q_10p }, { 20L, q_20p }, { 30L, q_30p }, { 40L, q_40p }, { 50L, q_50p }, { 60L, q_60p }, { 70L, q_70p }, { 80L, q_80p }, { 90L, q_90p }, { 100L, q_100p } };
            QualityButtons[ProtocolManager.DEFAULT_QUALITY.RoundToNearestTen()].Checked = true;

            my_client_port = (ushort)rnd.Next(1, 50000);  // randomize any port for the client

            // receive packets
            handlePackets = new Thread(() => { PacketManager.ReceivePackets(0, PacketHandler); });
            handlePackets.Start();

            // receive keep-alive packets
            handleKeepAlive = new Thread(() => { PacketManager.ReceivePackets(0, KeepAliveHandler); });
            handleKeepAlive.Start();

            Packet arpRequest = ARPManager.Request(ConfigManager.IP, out bool sameSubnet);  // search for server's mac (when answer will be received -
                                                                                            // the client will automatically send SRT induction request
            PacketManager.SendPacket(arpRequest);

            externalConnection = !sameSubnet;

            if (!sameSubnet)
                CConsole.WriteLine("[Client] External server address\n", MessageType.txtWarning);

            InductionCheck();

            ImageDisplay.ImageBoxDisplayIn = VideoBox;
            ServerAliveChecker.LostConnection += Server_LostConnection;  // subscribe the event to avoid unexpectable server shutdown
        }

        /// <summary>
        /// Check if there is ARP response from the server (if there is response, server mac should be changed, otherwise, if the mac null, it's a sign that the server havn't responded
        /// </summary>
        private void InductionCheck()
        {
            int duration = 5;  // seconds to wait for SRT server response

            // Create a timer that will trigger the countdown
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                // Decrement the countdown duration
                duration--;

                // If the countdown has reached zero, stop the timer and print a message
                if (duration <= 0)
                {
                    timer.Stop();
                    if (!serverAlive)  // still null after 5 seconds
                    {
                        CConsole.WriteLine("[Client] Server isn't responding to INDUCTION request", MessageType.txtError);
                        MessageBox.Show("Server isn't responding to [SRT: Induction] request..", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1);
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every incoming packet
        /// </summary>
        /// <param name="packet">New given packet</param>
        private void PacketHandler(Packet packet)
        {
            if (packet.IsValidUDP(my_client_port, ConfigManager.PORT))  // UDP Packet
            {
                DecryptionNecessity(packet, out byte[] payload);

                if (Control.SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Control.Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Control.Handshake handshake_request = new Control.Handshake(payload);

                        server_sid = handshake_request.SOURCE_SOCKET_ID;  // as first packet, we are setting the socket id to know it for the future

                        if (handshake_request.TYPE == (uint)Control.Handshake.HandshakeType.INDUCTION)  // (SRT) Induction
                        {
                            Console.WriteLine($"[Handshake] Got Induction: {handshake_request}\n");

                            serverAlive = true;
                            RequestsHandler.HandleInduction(handshake_request);
                        }
                        else if (handshake_request.TYPE == (uint)Control.Handshake.HandshakeType.CONCLUSION)  // (SRT) Conclusion 
                        {
                            Console.WriteLine($"[Handshake] Got Conclusion: {handshake_request}\n");

                            // encryption data received - initialize him for future decrypt necessity
                            Server_EncryptionControl = EncryptionFactory.CreateEncryption((EncryptionType)handshake_request.ENCRYPTION_TYPE, handshake_request.ENCRYPTION_PEER_PUBLIC_KEY);

                            Invoke((MethodInvoker)delegate
                            {
                                VideoBox.Text = "";
                            });

                            CConsole.WriteLine("[Handshake completed] Starting video display\n", MessageType.bgSuccess);
                            videoStage = true;
                            EnableQualityButtons();
                        }
                    }
                    else if (Control.Shutdown.IsShutdown(payload))  // (SRT) Server Shutdown ! [HANDLES ONLY CTRL + C EVENT ON SERVER SIDE] !
                        RequestsHandler.HandleShutDown();
                }

                else if (Data.SRTHeader.IsData(payload))  // (SRT) Data (chunk of image)
                {
                    ServerAliveChecker.Check();
                    Data.SRTHeader data_request = new Data.SRTHeader(payload);
#if DEBUG
                    Console.Title = $"Data received {++dataReceived}";
#endif
                    RequestsHandler.HandleData(data_request);
                }
            }

            else if (packet.IsValidARP())  // ARP Packet
            {
                ArpDatagram arp = packet.Ethernet.Arp;

                if (MethodExt.GetFormattedMac(arp.TargetHardwareAddress) == NetworkManager.MacAddress && !handledArp)  // my mac, and this is the first time answering 
                {
                    if ((arp.SenderProtocolIpV4Address.ToString() == ConfigManager.IP) || (arp.SenderProtocolIpV4Address.ToString() == NetworkManager.DefaultGateway)) // mac from server
                    {
                        // After client got the server's mac, send the first induction message
                        server_mac = MethodExt.GetFormattedMac(arp.SenderHardwareAddress);
                        CConsole.WriteLine($"[Client] Server/Gateway MAC Found: {server_mac}\n", MessageType.txtSuccess);
                        my_client_sid = ProtocolManager.GenerateSocketId(GetAdaptedIP(), my_client_port.ToString());

                        RequestsHandler.HandleArp(server_mac, my_client_port, my_client_sid);
                        handledArp = true;
                    }
                }
            }
        }

        /// <summary>
        /// If the client is in the video stage, and he enabled encryption, he should to decrypt each packet which is received from the server (only KeepAlive packets still raw)
        /// (according the after-video policy)
        /// </summary>
        /// <param name="packet">packet to check his state</param>
        /// <param name="payload">payload which is returned after check (raw/decrypted)</param>
        private void DecryptionNecessity(Packet packet, out byte[] payload)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            payload = datagram.Payload.ToArray();

            if (videoStage && ENCRYPTION != EncryptionType.None)  // if video stage reached and the encryption enabled -
                                                                  // the server will send each packet encrypted (data/shutdown/keepalive)
            {
                if (!Enum.IsDefined(typeof(EncryptionType), ENCRYPTION))
                    throw new Exception($"'{ENCRYPTION}' This encryption method isn't supported yet");

                payload = Server_EncryptionControl.TryDecrypt(payload);
            }
        }

        /// <summary>
        /// When video staged achieved, the quality buttons should be enabled 
        /// </summary>
        private void EnableQualityButtons()
        {
            foreach (ToolStripMenuItem button in QualityButtons.Values)
            {
                if (QualitySetter.InvokeRequired && QualitySetter.IsHandleCreated)
                {
                    QualitySetter.Invoke((MethodInvoker)delegate
                    {
                        button.Enabled = true;
                    });
                }
                else
                    button.Enabled = true;
                     
            }
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every keep alive packets
        /// </summary>
        /// <param name="packet">New given keepalive packet</param>
        private void KeepAliveHandler(Packet packet)
        {
            if (packet.IsValidUDP(my_client_port, ConfigManager.PORT))  // UDP Packet
            {
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (Control.SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Control.KeepAlive.IsKeepAlive(payload))
                    {
                        Debug.WriteLine("[KEEP-ALIVE] Received request\n");

                        KeepAliveRequest keepAlive_response = new KeepAliveRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, server_mac, NetworkManager.LocalIp, ConfigManager.IP, my_client_port, ConfigManager.PORT));
                        Packet keepAlive_confirm = keepAlive_response.Alive(server_sid, my_client_sid);
                        PacketManager.SendPacket(keepAlive_confirm);

                        Debug.WriteLine("[KEEP-ALIVE] Sending confirm\n");

                        CConsole.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Keep-Alive] Confirmed\n", MessageType.txtInfo);
                    }
                }
            }
        }

        /// <summary>
        /// If the connection is external (the server outside client's subnet) so use the public ip as client ip peer ip (peer ip is the packet sender IP according SRT docs)
        /// </summary>
        /// <returns>Adapted ip according the connection type</returns>
        internal static string GetAdaptedIP()
        {
            return externalConnection ? NetworkManager.PublicIp : NetworkManager.LocalIp;
        }

        /// <summary>
        /// If the server dies, notify the client and stop the session
        /// </summary>
        internal static void Server_LostConnection()
        {
            CConsole.WriteLine("[ERROR] Server isn't alive anymore", MessageType.bgError);

            MessageBox.Show("Lost connection with the server", "Connection Problem", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Environment.Exit(-1);
        }


        private void QualityChange_button_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem qualitySelected = (ToolStripMenuItem)sender;
            if (qualitySelected.Checked)  // if already selected - do not do anything
                return;

            foreach (ToolStripMenuItem item in QualityButtons.Values)  // uncheck all quality buttons
            {
                item.Checked = false;
            }

            qualitySelected.Checked = true;  // check the selected one 

            long newQuality = QualityButtons.FirstOrDefault(quality => quality.Value == qualitySelected).Key;

            QualityUpdateRequest qualityUpdate_request = new QualityUpdateRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));
            Packet qualityUpdate_packet = qualityUpdate_request.UpdateQuality(server_sid, my_client_sid, newQuality);
            PacketManager.SendPacket(qualityUpdate_packet);

            CConsole.WriteLine($"[Quality Update] Quality updated to: {newQuality}%\n", MessageType.txtInfo);
            ImageDisplay.CurrentVideoQuality = newQuality;
        }

        private void AutoQualityControl_Click(object sender, EventArgs e)
        {
            AutoQualityControl = autoQualityControl.Checked;
            string flag = AutoQualityControl ? "enabled" : "disabled";
            CConsole.WriteLine($"[Quality Update] Auto quality control {flag}\n", MessageType.txtInfo);
        }

        /// <summary>
        /// The function accurs when the form is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server_mac != null)
            {
                // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
                ShutdownRequest shutdown_request = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, server_mac, NetworkManager.LocalIp, ConfigManager.IP, my_client_port, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_request.Shutdown(server_sid, my_client_sid);
                PacketManager.SendPacket(shutdown_packet);
            }

            handlePackets.Abort();
            handleKeepAlive.Abort();
        }

    }
}
