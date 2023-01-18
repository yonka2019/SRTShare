using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Control = SRTShareLib.SRTManager.ProtocolFields.Control;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;
using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Client
{
    public partial class MainView : Form
    {
        private readonly Thread pRecvThread;
        private readonly Thread pRecvKAThread;
        private readonly Random rnd = new Random();

        private bool serverAlive = false;

        internal static ushort myPort;
        internal static string server_mac = null;
        internal static uint client_socket_id = 0;  // the server sends this value
        private static uint server_socket_id = 0;  // we getting know this value on the indoction that the server returns to us

        private bool handledArp = false;  // to avoid secondly induction to server (only for LOOPBACK connections (same pc server/client))
        private bool alive = true;

        internal static bool externalConnection;

#if DEBUG
        private static ulong dataReceived = 0;
#endif

        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 50000);

            //  start receiving packets
            pRecvThread = new Thread(() =>
            {
                PacketManager.ReceivePackets(0, PacketHandler);
            });
            pRecvThread.Start();

            //  start receiving keep-alive packets
            pRecvKAThread = new Thread(() =>
            {
                PacketManager.ReceivePackets(0, KeepAliveHandler);
            });
            pRecvKAThread.Start();

            Packet arpRequest = ARPManager.Request(ConfigManager.IP, out bool sameSubnet); // search for server's mac
            PacketManager.SendPacket(arpRequest);

            externalConnection = !sameSubnet;

            if (!sameSubnet)
                CConsole.WriteLine("[Client] External server address\n", MessageType.txtWarning);

            ResponseCheck();
        }

        /// <summary>
        /// Check if there is ARP response from the server (if there is response, server mac should be changed, otherwise, if the mac null, it's a sign that the server havn't responded
        /// </summary>
        private void ResponseCheck()
        {
            int duration = 5;  // seconds to wait

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
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (Control.SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Control.Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Control.Handshake handshake_request = new Control.Handshake(payload);

                        server_socket_id = handshake_request.SOCKET_ID;  // as first packet, we are setting the socket id to know it for the future

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

                        }
                    }
                }

                else if (Data.SRTHeader.IsData(payload))
                {
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

                if (MethodExt.GetFormattedMac(arp.TargetHardwareAddress) == PacketManager.MacAddress && !handledArp)  // my mac, and this is the first time answering 
                {
                    if ((arp.SenderProtocolIpV4Address.ToString() == ConfigManager.IP) || (arp.SenderProtocolIpV4Address.ToString() == PacketManager.DefaultGateway)) // mac from server
                    {
                        // After client got the server's mac, send the first induction message
                        server_mac = MethodExt.GetFormattedMac(arp.SenderHardwareAddress);
                        CConsole.WriteLine($"[Client] Server/Gateway MAC Found: {server_mac}\n", MessageType.txtSuccess);
                        client_socket_id = ProtocolManager.GenerateSocketId(GetAdaptedPeerIp(), myPort);

                        RequestsHandler.HandleArp(server_mac, myPort, client_socket_id);
                        handledArp = true;
                    }
                }
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
                        Debug.WriteLine("[GOT] Keep-Alive");
                        if (alive) // if client still alive, it will send a keep-alive response
                        {
                            KeepAliveRequest keepAlive_response = new KeepAliveRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, MainView.server_mac, PacketManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));
                            Packet keepAlive_confirm = keepAlive_response.Alive(server_socket_id);
                            PacketManager.SendPacket(keepAlive_confirm);
                            Debug.WriteLine("[SEND] Keep-Alive Confirm\n--------------------\n");
                        }
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
            if (server_mac != null)
            {
                // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
                ShutDownRequest shutdown_response = new ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, server_mac, PacketManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_response.Exit(server_socket_id);
                PacketManager.SendPacket(shutdown_packet);
            }

            alive = false;
            Environment.Exit(0);
            base.OnClosed(e);
        }

        /// <summary>
        /// If the connection is external (the server outside client's subnet) so use the public ip as peer ip (peer ip is the packet sender IP according SRT docs)
        /// </summary>
        /// <returns>Adapted ip according the connection type</returns>
        internal static string GetAdaptedPeerIp()
        {
            return externalConnection ? PacketManager.PublicIp : PacketManager.LocalIp;
        }
    }
}
