using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
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
        public Packet NotifyReceived(uint ack_sequence_number, uint dest_socket_id, uint source_socket_id, bool videoStage = false, PeerEncryptionData peerEncryption = default)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new ACK(dest_socket_id, ack_sequence_number, source_socket_id).GetByted(), videoStage, peerEncryption);
            return BuildPacket();
        }
    }
}