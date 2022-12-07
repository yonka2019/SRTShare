using PcapDotNet.Packets;

using SRTControl = SRTManager.ProtocolFields.Control;

namespace SRTManager.RequestsFactory
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
        public Packet Induction(uint cookie, uint init_psn, byte[] p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id)
        {
            SRTControl.Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (first message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new SRTControl.Handshake(version: 4, encryption_field: 0, init_psn, type: (uint)SRTControl.Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, 0, p_ip);
            }

            else
            {
                // LISTENER -> CALLER (first message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)SRTControl.Handshake.HandshakeType.INDUCTION, source_socket_id, dest_socket_id, cookie, p_ip);
            }

            GetPayloadLayer() = PacketManager.BuildPLayer(F_Handshake.GetByted()); // set last payload layer as our srt packet

            return BuildPacket();
        }


        public Packet Conclusion(uint init_psn, byte[] p_ip, bool clientSide, uint source_socket_id, uint dest_socket_id, uint cookie = 0)
        {
            SRTControl.Handshake F_Handshake;

            if (clientSide)
            {
                // CALLER -> LISTENER (second message REQUEST) [CLIENT -> SERVER]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)SRTControl.Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, cookie, p_ip);
            }

            else
            {
                // LISTENER -> CALLER (second message RESPONSE) [SERVER -> CLIENT]
                F_Handshake = new SRTControl.Handshake(version: 5, encryption_field: 0, init_psn, type: (uint)SRTControl.Handshake.HandshakeType.CONCLUSION, source_socket_id, dest_socket_id, 0, p_ip);
            }

            GetPayloadLayer() = PacketManager.BuildPLayer(F_Handshake.GetByted()); // set last payload layer as our srt packet

            return BuildPacket();
        }

    }
}
