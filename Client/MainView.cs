using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using SRTLibrary.SRTManager.RequestsFactory;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

/*
 * PACKET STRUCTURE:
 * // [PACKET ID (CHUNK NUMBER)]  [TOTAL CHUNKS NUMBER]  [DATA / LAST DATA] //
 * //       [2 BYTES]                   [2 BYTES]          [>=1000 BYTES]   //
 */

namespace Client
{
    public partial class MainView : Form
    {
        private readonly Thread pRecvThread;
        private readonly Random rnd = new Random();

        internal static ushort myPort = 0;
        internal static string server_mac;
        internal static uint client_socket_id = 0;

        private static uint server_socket_id = 0;
        private bool first = true;

        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 50000);

            //  start receiving packets
            pRecvThread = new Thread(new ThreadStart(RecvP));
            pRecvThread.Start();

            Packet arpRequest = ARPManager.Request(PacketManager.device, PacketManager.SERVER_IP);
            PacketManager.SendPacket(arpRequest);
        }

        /// <summary>
        /// The function starts receiving the packets
        /// </summary>
        private void RecvP()
        {
            PacketManager.ReceivePackets(0, PacketHandler);
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every incoming packet
        /// </summary>
        /// <param name="packet">New given packet</param>
        private void PacketHandler(Packet packet)
        {
            if (packet.IsValidUDP(myPort, PacketManager.SERVER_PORT))  // UDP Packet
            {
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Handshake handshake_request = new Handshake(payload);

                        server_socket_id = handshake_request.SOCKET_ID;  // as first packet, we are setting the socket id to know it for the future

                        if (handshake_request.TYPE == (uint)Handshake.HandshakeType.INDUCTION)  // [server -> client] (SRT) Induction
                        {
                            RequestsHandler.HandleInduction(handshake_request);
                        }
                    }
                    else if (KeepAlive.IsKeepAlive(payload))
                    {

                        KeepAliveRequest keepAlive_response = new KeepAliveRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, MainView.server_mac, PacketManager.localIp, PacketManager.SERVER_IP, MainView.myPort, PacketManager.SERVER_PORT));
                        Packet keepAlive_confirm = keepAlive_response.Check(server_socket_id);
                        PacketManager.SendPacket(keepAlive_confirm);
                    }
                }
            }
            else if (packet.IsValidARP())  // ARP Packet
            {
                ArpDatagram arp = packet.Ethernet.Arp;

                if (IsMyMac(arp) && first) // my mac, and this is the first time answering 
                {
                    if (arp.SenderProtocolIpV4Address.ToString() == PacketManager.SERVER_IP) // mac from server
                    {
                        server_mac = BitConverter.ToString(arp.SenderHardwareAddress.ToArray()).Replace("-", ":");

                        HandshakeRequest handshake = new HandshakeRequest
                    (PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));

                        //create induction packet
                        DateTime now = DateTime.Now;

                        client_socket_id = ProtocolManager.GenerateSocketId(PacketManager.localIp, myPort);

                        IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
                        Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.localIp, myPort, now), init_psn: 0, p_ip: peer_ip, clientSide: true, client_socket_id, 0);

                        PacketManager.SendPacket(handshake_packet);

                        first = false;
                    }
                }
            }

        }

        /// <summary>
        /// The function checks if the targeted mac is my mac
        /// </summary>
        /// <param name="arp">ArpDatagram object</param>
        /// <returns>True if it's my mac, false if not</returns>
        private bool IsMyMac(ArpDatagram arp)
        {
            return BitConverter.ToString(arp.TargetHardwareAddress.ToArray()).Replace("-", ":") == ARPManager.GetMyMac(PacketManager.device);
        }

        /// <summary>
        /// The function accurs when the form is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
            ShutDownRequest shutdown_response = new ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));
            Packet shutdown_packet = shutdown_response.Exit();
            PacketManager.SendPacket(shutdown_packet);

            MessageBox.Show("Sent a ShutDown request!",
                "Bye Bye", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            base.OnClosed(e);
        }
    }
}
