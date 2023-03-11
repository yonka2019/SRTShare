using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.ProtocolFields.Control;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class ACKRequest : UdpPacket
    {
        public ACKRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a ack packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>An ack packet</returns>
        public Packet ConfirmReceivedImage(uint confirmed_sequence_number, uint dest_socket_id, uint source_socket_id)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new ACK(dest_socket_id, source_socket_id, confirmed_sequence_number).GetByted());
            return BuildPacket();
        }
    }
}