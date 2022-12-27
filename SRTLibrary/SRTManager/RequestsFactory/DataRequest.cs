using PcapDotNet.Packets;
using System.Collections.Generic;
using SRTData = SRTLibrary.SRTManager.ProtocolFields.Data;


namespace SRTLibrary.SRTManager.RequestsFactory
{
    public class DataRequest : UdpPacket
    {
        public DataRequest(params ILayer[] layers) : base(layers) { }

        public List<Packet> SplitToPackets(List<byte> stream, uint time_stamp, uint dest_socket_id, int MTU)
        {
            List<Packet> packets = new List<Packet>();
            List<byte> packet_data;
            SRTData.SRTHeader srt_packet_data;

            int i;

            for (i = MTU; (i + MTU) < stream.Count; i += MTU) // MTU bytes iterating
            {
                packet_data = stream.GetRange(i - MTU, MTU);

                SRTData.PositionFlags packetPositionFlag = (i == MTU) ? SRTData.PositionFlags.FIRST : SRTData.PositionFlags.MIDDLE;

                srt_packet_data = new SRTData.SRTHeader(sequence_number: 0, packetPositionFlag, SRTData.EncryptionFlags.NOT_ENCRYPTED, is_retransmitted: false, message_number: 0, time_stamp, dest_socket_id, packet_data);
                GetPayloadLayer() = PacketManager.BuildPLayer(srt_packet_data.GetByted());

                packets.Add(BuildPacket());
            }

            packet_data = stream.GetRange(i, stream.Count - i);

            srt_packet_data = new SRTData.SRTHeader(sequence_number: 0, SRTData.PositionFlags.LAST, SRTData.EncryptionFlags.NOT_ENCRYPTED, is_retransmitted: false, message_number: 0, time_stamp, dest_socket_id, packet_data);
            GetPayloadLayer() = PacketManager.BuildPLayer(srt_packet_data.GetByted());

            packets.Add(BuildPacket());

            return packets;
        }
    }
}
