using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using SRTLibrary.SRTManager.RequestsFactory;
using System;

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
            uint client_id = ProtocolManager.GenerateSocketId(packet.Ethernet.IpV4.Source.ToString(), packet.Ethernet.Ip.Udp.SourcePort);

            if (Program.SRTSockets.ContainsKey(client_id))
            {
                Console.WriteLine($"Got a Shutdown Request from: {Program.SRTSockets[client_id].SocketAddress.IPAddress}.");

                Program.Dispose(client_id);
            }
        }


        /// <summary>
        /// The function handles what happens after getting an induction message from a client
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        /// <param name="datagram">The transport layer</param>
        internal static void HandleInduction(Packet packet, Handshake handshake_request)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.MacAddress, packet.Ethernet.Source.ToString(), PacketManager.LocalIp, packet.IpV4.Source.ToString(), ConnectionConfig.SERVER_PORT, datagram.SourcePort));

            string client_ip = handshake_request.PEER_IP.ToString();
            uint cookie = ProtocolManager.GenerateCookie(client_ip, datagram.SourcePort); // need to save cookie somewhere

            IpV4Address peer_ip = new IpV4Address(PacketManager.LocalIp);
            Packet handshake_packet = handshake_response.Induction(cookie, init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID); // ***need to change peer id***
            PacketManager.SendPacket(handshake_packet);

            Console.WriteLine("\n\nInduction [Client -> Server]:\n" + handshake_request + "\n--------------------\n\n");
        }

        /// <summary>
        /// The function handles what happens after getting an conclusion message from a client
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        /// <param name="datagram">The transport layer</param>
        internal static void HandleConclusion(Packet packet, Handshake handshake_request)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.MacAddress, packet.Ethernet.Source.ToString(), PacketManager.LocalIp, packet.IpV4.Source.ToString(), ConnectionConfig.SERVER_PORT, datagram.SourcePort));

            IpV4Address peer_ip = new IpV4Address(PacketManager.LocalIp);
            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID); // ***need to change peer id***
            PacketManager.SendPacket(handshake_packet);

            Console.WriteLine("\n\nConclusion [Client -> Server]:\n" + handshake_request + "\n--------------------\n\n");

            // ADD NEW SOCKET TO LIST 
            SClient currentClient = new SClient(handshake_request.PEER_IP, datagram.SourcePort, packet.Ethernet.Source, handshake_request.SOCKET_ID, handshake_request.MTU);

            KeepAliveManager kaManager = new KeepAliveManager(currentClient);
            DataManager dataManager = new DataManager(currentClient);

            Program.SRTSockets.Add(handshake_request.SOCKET_ID, new SRTSocket(currentClient,
                kaManager, dataManager));
            kaManager.LostConnection += Program.LostConnection;
        }

        /// <summary>
        /// The function handles with the arp request from the client
        /// </summary>
        /// <param name="packet">Received packet</param>
        internal static void HandleArp(Packet packet)
        {
            ArpDatagram arp = packet.Ethernet.Arp;
            Packet arpReply = ARPManager.Reply(MethodExt.GetValidMac(arp.SenderHardwareAddress), arp.SenderProtocolIpV4Address.ToString());
            PacketManager.SendPacket(arpReply);
        }
    }
}
