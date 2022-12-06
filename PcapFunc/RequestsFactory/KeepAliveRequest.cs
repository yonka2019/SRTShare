using PcapDotNet.Packets;

using SRTControl = SRTManager.ProtocolFields.Control;

namespace SRTManager.RequestsFactory
{
    public class KeepAliveRequest : UdpPacket
    {
        public KeepAliveRequest(params ILayer[] layers) : base(layers) { }

        public Packet Check(uint dest_socket_id)
        {
            GetPayloadLayer() = PacketManager.BuildPLayer(new SRTControl.KeepAlive(dest_socket_id).GetByted());
            return BuildPacket();
        }
    }
}
