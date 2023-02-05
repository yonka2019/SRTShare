using PcapDotNet.Packets;
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

        internal static uint server_sid = 0;  // we getting know this value on the indoction that the server returns to us (SID -> Socket ID)
        internal static ushort myPort;
        internal static uint client_sid = 0;  // the server sends this value on the induction answer (SID -> Socket ID) (MY SID)
        internal static string serverMac = null;
        internal static bool externalConnection;

        private static Dictionary<EncryptionType, Func<string, (byte[], byte[])>> EncCredFunc;  // Functions which is responsible for getting the key +/ iv 

        private static Thread handlePackets, handleKeepAlive;
#if DEBUG
        private static ulong dataReceived = 0;  // count data packets received (included chunks)
#endif

        //  - CONVERSATION SETTINGS - + - + - + - + - + - + - + - +

        internal const EncryptionType ENCRYPTION = EncryptionType.Substitution;  // The whole encryption of the conversation (from data stage)
        internal const int INITIAL_PSN = 0;  // The first sequence number of the conversation

        //  - CONVERSATION SETTINGS - + - + - + - + - + - + - + - +

        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 50000);  // randomize any port for the client

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

            ServerAliveChecker.LostConnection += Server_LostConnection;  // subscribe the event to avoid unexpectable server shutdown

            RegisterEncKeysFunctions();
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
            if (packet.IsValidUDP(myPort, ConfigManager.PORT))  // UDP Packet
            {
                DecryptionNecessity(packet, out byte[] payload);

                if (Control.SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Control.Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Control.Handshake handshake_request = new Control.Handshake(payload);

                        server_sid = handshake_request.SOCKET_ID;  // as first packet, we are setting the socket id to know it for the future

                        if (handshake_request.TYPE == (uint)Control.Handshake.HandshakeType.INDUCTION)  // (SRT) Induction
                        {
                            serverAlive = true;
                            RequestsHandler.HandleInduction(handshake_request);
                        }
                        else if (handshake_request.TYPE == (uint)Control.Handshake.HandshakeType.CONCLUSION)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                VideoBox.Text = "";
                            });
                            CConsole.WriteLine("[Handshake completed] Starting video display\n", MessageType.bgSuccess);
                            videoStage = true;
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
                    RequestsHandler.HandleData(data_request, VideoBox);
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
                        serverMac = MethodExt.GetFormattedMac(arp.SenderHardwareAddress);
                        CConsole.WriteLine($"[Client] Server/Gateway MAC Found: {serverMac}\n", MessageType.txtSuccess);
                        client_sid = ProtocolManager.GenerateSocketId(GetAdaptedPeerIp());

                        RequestsHandler.HandleArp(serverMac, myPort, client_sid);
                        handledArp = true;
                    }
                }
            }
        }

        /// <summary>
        /// If the client is in the video stage, and he enabled encryption, he should to decrypt each packet which is received from the server
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
                if (!EncCredFunc.ContainsKey(ENCRYPTION))
                    throw new Exception($"'{ENCRYPTION}' This encryption method isn't supported yet");

                string ip = packet.Ethernet.IpV4.Destination.ToString();

                (byte[] key, byte[] IV) = EncCredFunc[ENCRYPTION](ip);

                payload = EncryptionManager.TryDecrypt(ENCRYPTION, payload, key, IV);
            }
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every keep alive packets
        /// </summary>
        /// <param name="packet">New given keepalive packet</param>
        private void KeepAliveHandler(Packet packet)
        {
            if (packet.IsValidUDP(myPort, ConfigManager.PORT))  // UDP Packet
            {
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (Control.SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Control.KeepAlive.IsKeepAlive(payload))
                    {
                        Debug.WriteLine("[KEEP-ALIVE] Received request\n");

                        KeepAliveRequest keepAlive_response = new KeepAliveRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, serverMac, NetworkManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));
                        Packet keepAlive_confirm = keepAlive_response.Alive(server_sid);
                        PacketManager.SendPacket(keepAlive_confirm);

                        Debug.WriteLine("[KEEP-ALIVE] Sending confirm\n");
                    }
                }
            }
        }

        /// <summary>
        /// The function accurs when the form is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            if (serverMac != null)
            {
                // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
                ShutdownRequest shutdown_request = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, serverMac, NetworkManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_request.Shutdown(server_sid);
                PacketManager.SendPacket(shutdown_packet);
            }

            handlePackets.Abort();
            handleKeepAlive.Abort();

            Environment.Exit(0);
            base.OnClosed(e);
        }

        /// <summary>
        /// If the connection is external (the server outside client's subnet) so use the public ip as peer ip (peer ip is the packet sender IP according SRT docs)
        /// </summary>
        /// <returns>Adapted ip according the connection type</returns>
        internal static string GetAdaptedPeerIp()
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

        /// <summary>
        /// Register encryption create keys functions individually for each encryption type
        /// </summary>
        public static void RegisterEncKeysFunctions()
        {
            EncCredFunc = new Dictionary<EncryptionType, Func<string, (byte[], byte[])>>
            {
                { EncryptionType.AES128, AES128.CreateKey_IV },
                { EncryptionType.Substitution, Substitution.CreateKey },
                { EncryptionType.XOR, XOR.CreateKey }
            };
        }
    }
}
