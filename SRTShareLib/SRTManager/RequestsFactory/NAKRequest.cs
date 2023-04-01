using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.ProtocolFields.Control;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class NAKRequest : UdpPacket
    {
        public NAKRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a nak packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A nak packet</returns>
        public Packet RequestRetransmit(uint corrupted_sequence_number, uint dest_socket_id, uint source_socket_id)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new NAK(dest_socket_id, source_socket_id, corrupted_sequence_number).GetByted());
            return BuildPacket();
        }
    }
}