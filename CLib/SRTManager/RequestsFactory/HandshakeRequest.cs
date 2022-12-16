using CLib.SRTManager.ProtocolFields.Control;
using PcapDotNet.Packets;

using SRTControl = CLib.SRTManager.ProtocolFields.Control;

namespace CLib.SRTManager.RequestsFactory
{
    public class HandshakeRequest : UdpPacket
    {
        /*
         *  Usage Example:
                    var handshakeRequest = new SRTManager.ProtocolManager.Handshake(PacketManager.BuildEthernetLayer(),
                    PacketManager.BuildIpv4Layer(),
                    PacketManager.BuildUdpLayer(600, PacketManager.SERVER_PORT));
                    Packet readyToSent = a.Induction("a", 1, false);
        */

        public HandshakeRequest(params ILayer[] layers) : base(layers) { }

        // public Handshake(uint version, ushort encryption_field, uint intial_psn, uint type, uint socket_id, uint syn_cookie, decimal p_ip)

        /// <summary>
        /// The function creates an induction packet (Handshake)
        /// </summary>
        /// <param name="cookie">Cookie</param>
        /// <param name="init_psn">Init psn</param>
        /// <param name="p_ip">Peer ip</param>
        /// <param name="clientSide">Whether it's the client side or not</param>
        /// <param name="source_socket_id">Source socket id</param>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>Induction packet</returns>
        public Packet Induction(uint cookie, uint init_psn, uint p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id)
        {
            Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (first message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new SRTControl.Handshake(version: 4, encryption_field: 0, init_psn, type: (uint)Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, 0, p_ip);
            }

            else
            {
                // LISTENER -> CALLER (first message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, cookie, p_ip);
            }

            GetPayloadLayer() = PacketManager.BuildPLayer(F_Handshake.GetByted()); // set last payload layer as our srt packet

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
        /// <returns>Conclusion packet</returns>
        public Packet Conclusion(uint init_psn, uint p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id, uint cookie = 0)
        {
            Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (second message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, cookie, p_ip);
            }

            else
            {
                // LISTENER -> CALLER (second message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, 0, p_ip);
            }

            GetPayloadLayer() = PacketManager.BuildPLayer(F_Handshake.GetByted()); // set last payload layer as our srt packet

            return BuildPacket();
        }

    }
}
