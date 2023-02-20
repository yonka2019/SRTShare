using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using System.Collections.Generic;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class NakRequest : UdpPacket
    {
        public NakRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a nak packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A nak packet</returns>
        public Packet SendMissingPackets(List<uint> lost_packets, uint dest_socket_id = 0, bool videoStage = false, EncryptionType encryptionType = EncryptionType.None)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new Nak(dest_socket_id, lost_packets).GetByted(), videoStage, encryptionType, GetLayers());
            return BuildPacket();
        }
    }
}
