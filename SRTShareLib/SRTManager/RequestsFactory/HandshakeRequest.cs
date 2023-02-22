﻿using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.ProtocolFields.Control;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class HandshakeRequest : UdpPacket
    {
        public HandshakeRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates an induction packet (Handshake)
        /// </summary>
        /// <param name="cookie">Cookie</param>
        /// <param name="init_psn">Init psn</param>
        /// <param name="p_ip">Peer ip</param>
        /// <param name="clientSide">Whether it's the client side or not</param>
        /// <param name="source_socket_id">Source socket id</param>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <param name="encryption_type">selected encryption type</param>
        /// <param name="encryption_public_key">the encryption key (in induction - null)</param>
        /// <returns>Induction packet</returns>
        public Packet Induction(uint cookie, uint init_psn, IpV4Address p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id, ushort encryption_type, byte[] encryption_public_key)
        {
            Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (first message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new Handshake(version: 4, encryption_type, encryption_public_key, init_psn, type: (uint)Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, 0, p_ip);
            }
            else
            {
                // LISTENER -> CALLER (first message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new Handshake(version: 5, encryption_type, encryption_public_key, init_psn, type: (uint)Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, cookie, p_ip);
            }

            GetPayloadLayer() = OSIManager.BuildPLayer(F_Handshake.GetByted(), false);  // set last payload layer as our srt packet

            return BuildPacket();
        }
            
        /// <summary>
        /// The function creates a conclusion packet (Handshake)
        /// </summary>
        /// <param name="init_psn">Init psn</param>
        /// <param name="p_ip">Peer ip</param>
        /// <param name="clientSide">Whether it's the client side or not</param>
        /// <param name="source_socket_id">Source socket id</param>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <param name="cookie">Cookie</param>
        /// <param name="encryption_type">selected encryption type</param>
        /// <param name="encryption_public_key">the encryption key (256 bits / 32 bytes)</param>
        /// <returns>Conclusion packet</returns>
        public Packet Conclusion(uint init_psn, IpV4Address p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id, ushort encryption_type, byte[] encryption_public_key, uint cookie = 0)
        {
            Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (second message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new Handshake(version: 5, encryption_type, encryption_public_key, init_psn, type: (uint)Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, cookie, p_ip);
            }
            else
            {
                // LISTENER -> CALLER (second message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new Handshake(version: 5, encryption_type, encryption_public_key, init_psn, type: (uint)Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, 0, p_ip);
            }

            GetPayloadLayer() = OSIManager.BuildPLayer(F_Handshake.GetByted(), false);  // set last payload layer as our srt packet

            return BuildPacket();
        }
    }
}
