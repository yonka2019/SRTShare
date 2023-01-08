using PcapDotNet.Packets;
using SRTControl = SRTLibrary.SRTManager.ProtocolFields.Control;


namespace SRTLibrary.SRTManager.RequestsFactory
{
    public class KeepAliveRequest : UdpPacket
    {
        public KeepAliveRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a keep alive packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A keep alive packet</returns>
        public Packet Check(uint dest_socket_id)
        {
            GetPayloadLayer() = PacketManager.BuildPLayer(new SRTControl.KeepAlive(dest_socket_id).GetByted());
            return BuildPacket();
        }
    }
}
