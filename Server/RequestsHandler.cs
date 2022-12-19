using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using SRTLibrary.SRTManager.RequestsFactory;
using System;
using SRTRequest = SRTLibrary.SRTManager.RequestsFactory;

namespace Server
{
    internal class RequestsHandler
    {
        /// <summary>
        /// The function handles the shutdown
        /// </summary>
        /// <param name="packet">packet of shutdown</param>
        internal static void HandleShutDown(Packet packet)
        {
            uint client_id = ProtocolManager.GenerateSocketId(packet.Ethernet.IpV4.Source.ToString(), packet.Ethernet.Ip.Udp.SourcePort);

            Console.WriteLine($"Got a Shutdown Request from: {client_id}.");

            if (Program.SRTSockets.ContainsKey(client_id))
            {
                Program.SRTSockets.Remove(client_id);
                Console.WriteLine($"Client [{client_id}] was removed.");
            }

            else
                Console.WriteLine($"Client [{client_id}] wasn't found.");
        }

        /// <summary>
        /// The function handles the induction phaze
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        /// <param name="datagram">The transport layer</param>
        internal static void HandleInduction(Packet packet, Handshake handshake_request, UdpDatagram datagram)
        {
            HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.macAddress, packet.Ethernet.Source.ToString(), PacketManager.localIp, packet.IpV4.Source.ToString(), PacketManager.SERVER_PORT, datagram.SourcePort));

            string client_ip = handshake_request.PEER_IP.ToString();
            uint cookie = ProtocolManager.GenerateCookie(client_ip, datagram.SourcePort, DateTime.Now); // need to save cookie somewhere

            IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
            Packet handshake_packet = handshake_response.Induction(cookie, init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID); // ***need to change peer id***
            PacketManager.SendPacket(handshake_packet);

            Console.WriteLine("Induction [Client -> Server]:\n" + handshake_request + "\n--------------------\n\n");
        }

        /// <summary>
        /// The function handles the conclusion phaze
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        /// <param name="datagram">The transport layer</param>
        internal static void HandleConclusion(Packet packet, Handshake handshake_request, UdpDatagram datagram)
        {
            HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.macAddress, packet.Ethernet.Source.ToString(), PacketManager.localIp, packet.IpV4.Source.ToString(), PacketManager.SERVER_PORT, datagram.SourcePort));

            IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOCKET_ID); // ***need to change peer id***
            PacketManager.SendPacket(handshake_packet);

            Console.WriteLine("Conclusion [Client -> Server]:\n" + handshake_request + "\n--------------------\n\n");

            // ADD NEW SOCKET TO LIST 
            SClient currentClient = new SClient(handshake_request.PEER_IP, datagram.SourcePort, packet.Ethernet.Source, handshake_request.SOCKET_ID);

            Program.SRTSockets.Add(handshake_request.SOCKET_ID, new SRTSocket(currentClient,
                new KeepAliveManager(currentClient)));
        }
    }
}
