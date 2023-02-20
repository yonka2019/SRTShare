using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using System.Collections.Generic;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    internal class AckRequest : UdpPacket
    {
        public AckRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a ack packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>An ack packet</returns>
        public Packet NotifyReceived(uint ack_sequence_number, uint dest_socket_id = 0, bool videoStage = false, EncryptionType encryptionType = EncryptionType.None)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new Ack(dest_socket_id, ack_sequence_number).GetByted(), videoStage, encryptionType, GetLayers());
            return BuildPacket();
        }
    }
}
