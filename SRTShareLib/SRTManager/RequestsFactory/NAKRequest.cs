using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using System.Collections.Generic;

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
        public Packet SendMissingPackets(uint corrupted_sequence_number, List<uint> lost_packets, uint dest_socket_id, uint source_socket_id, bool videoStage = false, BaseEncryption baseEncryption = null)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new NAK(dest_socket_id, source_socket_id, corrupted_sequence_number, lost_packets).GetByted(), videoStage, baseEncryption);
            return BuildPacket();
        }
    }
}