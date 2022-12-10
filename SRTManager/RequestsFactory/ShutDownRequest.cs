using PcapDotNet.Packets;

using SRTControl = SRTManager.ProtocolFields.Control;

namespace SRTManager.RequestsFactory
{
    public class ShutDownRequest : UdpPacket
    {
        public ShutDownRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a shutdown packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A shutdown packet</returns>
        public Packet Exit(uint dest_socket_id = 0)
        {
            GetPayloadLayer() = PacketManager.BuildPLayer(new SRTControl.Shutdown(dest_socket_id).GetByted());
            return BuildPacket();
        }

    }
}
