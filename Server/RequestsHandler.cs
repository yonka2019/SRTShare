using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using SRTShareLib.SRTManager.RequestsFactory;

using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Server
{
    internal class RequestsHandler
    {
        /// <summary>
        /// The function handles what happens after getting an induction message from a client
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        internal static void HandleInduction(Packet packet, Handshake handshake_request)  // [SERVER] -> [CLIENT]
        {
            if (Program.SRTSockets.ContainsKey(handshake_request.SOURCE_SOCKET_ID))
            {
                CConsole.WriteLine("[ERROR] This client (IP:PORT) is already connected (droping connection)", MessageType.txtError);
                return;
            }

            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, packet.Ethernet.Source.ToString(), NetworkManager.LocalIp, packet.Ethernet.IpV4.Source.ToString(), ConfigManager.PORT, datagram.SourcePort));

            IpV4Address peer_ip = new IpV4Address(NetworkManager.PublicIp);
            Packet handshake_packet = handshake_response.Induction(init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOURCE_SOCKET_ID, handshake_request.ENCRYPTION_TYPE, new byte[DiffieHellman.PUBLIC_KEY_SIZE], handshake_request.RETRANSMISSION_MODE);
            PacketManager.SendPacket(handshake_packet);
        }

        /// <summary>
        /// signing a client into the dictionaries, confirming connection
        /// </summary>
        /// <param name="packet">Given packet</param>
        /// <param name="handshake_request">The handshake object</param>
        internal static void HandleConclusion(Packet packet, Handshake handshake_request)  // [SERVER] -> [CLIENT]
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;

            HandshakeRequest handshake_response = new HandshakeRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, packet.Ethernet.Source.ToString(), NetworkManager.LocalIp, packet.Ethernet.IpV4.Source.ToString(), ConfigManager.PORT, datagram.SourcePort));

            IpV4Address peer_ip = new IpV4Address(NetworkManager.PublicIp);

            byte[] myPublicKey = new byte[DiffieHellman.PUBLIC_KEY_SIZE];
            if ((EncryptionType)handshake_request.ENCRYPTION_TYPE != EncryptionType.None)  // save peer (client) public key, send mine public key to him
                myPublicKey = DiffieHellman.MyPublicKey;

            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: false, Program.SERVER_SOCKET_ID, handshake_request.SOURCE_SOCKET_ID, handshake_request.ENCRYPTION_TYPE, myPublicKey, handshake_request.RETRANSMISSION_MODE);
            PacketManager.SendPacket(handshake_packet);

            #region New client information set

            SClient currentClient = new SClient(handshake_request.PEER_IP, datagram.SourcePort, packet.Ethernet.Source, handshake_request.SOURCE_SOCKET_ID, handshake_request.MTU);
            KeepAliveManager kaManager = new KeepAliveManager(currentClient);
            VideoManager videoManager = new VideoManager(currentClient, EncryptionFactory.CreateEncryption((EncryptionType)handshake_request.ENCRYPTION_TYPE, handshake_request.ENCRYPTION_PEER_PUBLIC_KEY), handshake_request.INTIAL_PSN, handshake_request.RETRANSMISSION_MODE);

            SRTSocket newSRTSocket = new SRTSocket(currentClient,
                kaManager, videoManager);

            // add client to sockets list
            Program.SRTSockets.Add(handshake_request.SOURCE_SOCKET_ID, newSRTSocket);

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

        internal static void HandleKeepAlive(KeepAlive keepAlive_request)
        {
            Program.SRTSockets[keepAlive_request.SOURCE_SOCKET_ID].KeepAlive.ConfirmStatus();  // sign as alive
        }

        internal static void HandleQualityUpdate(QualityUpdate qualityUpdate_request)
        {
            Program.SRTSockets[qualityUpdate_request.SOURCE_SOCKET_ID].Data.CurrentQuality = qualityUpdate_request.QUALITY;
        }

        internal static void HandleNAK(NAK NAK_request)
        {
            uint imageToTransmit = NAK_request.CORRUPTED_SEQUENCE_NUMBER;
            Program.SRTSockets[NAK_request.SOURCE_SOCKET_ID].Data.ResendImage(imageToTransmit);  // resend all the packets for the missing sequence number (image)
        }

        internal static void HandleACK(ACK ACK_request)
        {
            uint imageToConfirm = ACK_request.ACK_SEQUENCE_NUMBER;
            Program.SRTSockets[ACK_request.SOURCE_SOCKET_ID].Data.ConfirmImage(imageToConfirm);  // clear all the packets of teh received image sequence number
        }

        /// <summary>
        /// The function handles what happens after getting a shutdown message from a client
        /// </summary>
        internal static void HandleShutdown(Shutdown shutdown_request)
        {
            Program.DisposeClient(shutdown_request.SOURCE_SOCKET_ID);
        }
    }
}
