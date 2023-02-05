using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using SRTShareLib.SRTManager.RequestsFactory;
using System;

using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Server
{
    internal class RequestsHandler
    {
        /// <summary>
        /// The function handles what happens after getting a shutdown message from a client
        /// </summary>
        /// <param name="packet">packet of shutdown</param>
        internal static void HandleShutDown(Packet packet)
        {
            uint client_id = ProtocolManager.GenerateSocketId(packet.Ethernet.IpV4.Source.ToString());

            if (Program.SRTSockets.ContainsKey(client_id))
            {
                Console.WriteLine($"[Shutdown] Got shutdown request from: {Program.SRTSockets[client_id].SocketAddress.IPAddress}\n");

                Program.DisposeClient(client_id);
            }
        }

        /// <summary>
        /// The function handles what happens after getting an induction message from a client
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        internal static void HandleInduction(Packet packet, Handshake handshake_request)  // [SERVER] -> [CLIENT]
        {
            Console.WriteLine($"[Handshake] Got Induction: {handshake_request}\n");

            if (Program.SRTSockets.ContainsKey(handshake_request.SOCKET_ID))
            {
                CConsole.WriteLine("[ERROR] This IP is already connected (droping connection)", MessageType.txtError);
                return;
            }

            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, packet.Ethernet.Source.ToString(), NetworkManager.LocalIp, packet.Ethernet.IpV4.Source.ToString(), ConfigManager.PORT, datagram.SourcePort));

            string client_ip = handshake_request.PEER_IP.ToString();
            uint cookie = ProtocolManager.GenerateCookie(client_ip);

            IpV4Address peer_ip = new IpV4Address(NetworkManager.PublicIp);
            Packet handshake_packet = handshake_response.Induction(cookie, init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID, handshake_request.ENCRYPTION_FIELD);
            PacketManager.SendPacket(handshake_packet);
        }

        /// <summary>
        /// The function handles what happens after getting an conclusion message from a client
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        internal static void HandleConclusion(Packet packet, Handshake handshake_request)  // [SERVER] -> [CLIENT]
        {
            Console.WriteLine($"[Handshake] Got Conclusion: {handshake_request}\n");

            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, packet.Ethernet.Source.ToString(), NetworkManager.LocalIp, packet.Ethernet.IpV4.Source.ToString(), ConfigManager.PORT, datagram.SourcePort));

            IpV4Address peer_ip = new IpV4Address(NetworkManager.PublicIp);
            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID, handshake_request.ENCRYPTION_FIELD);
            PacketManager.SendPacket(handshake_packet);

            #region New client information set

            SClient currentClient = new SClient(handshake_request.PEER_IP, datagram.SourcePort, packet.Ethernet.Source, handshake_request.SOCKET_ID, handshake_request.MTU);
            KeepAliveManager kaManager = new KeepAliveManager(currentClient);
            VideoManager dataManager = new VideoManager(currentClient, handshake_request.ENCRYPTION_FIELD, handshake_request.INTIAL_PSN);

            SRTSocket newSRTSocket = new SRTSocket(currentClient,
                kaManager, dataManager);

            // add client to sockets list
            Program.SRTSockets.Add(handshake_request.SOCKET_ID, newSRTSocket);

            kaManager.LostConnection += Program.Client_LostConnection;

            #endregion
        }

        /// <summary>
        /// The function handles with the arp request from the client
        /// </summary>
        /// <param name="packet">Received packet</param>
        internal static void HandleArp(Packet packet)
        {
            ArpDatagram arp = packet.Ethernet.Arp;
            Packet arpReply = ARPManager.Reply(MethodExt.GetFormattedMac(arp.SenderHardwareAddress), arp.SenderProtocolIpV4Address.ToString());
            PacketManager.SendPacket(arpReply);
        }
    }
}
