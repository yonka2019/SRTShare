using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using System;
using System.Collections.Generic;
using SRTData = SRTShareLib.SRTManager.ProtocolFields.Data;


namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class DataRequest : UdpPacket
    {
        public DataRequest(params ILayer[] layers) : base(layers) { }

        public List<Packet> SplitToPackets(List<byte> stream, ref uint sequence_number, uint time_stamp, uint dest_socket_id, int MTU, BaseEncryption baseEncryption, bool retransmitted)
        {
            List<Packet> packets = new List<Packet>();
            List<byte> packet_data;
            SRTData.SRTHeader srt_packet_data;
            SRTData.PositionFlags packetPositionFlag;

            int i = 0;
            uint messageNumber = 0;

            while (i < stream.Count)  // Iterate until all bytes in the stream have been processed
            {
                int packetLength = Math.Min(MTU, stream.Count - i);  // Calculate the length of the packet to be sent
                packet_data = stream.GetRange(i, packetLength);  // Get the packet data from the stream

                if (stream.Count <= MTU)  // stream is not bigger than the MTU, there is no reason to split the image to chunks
                    packetPositionFlag = SRTData.PositionFlags.SINGLE_DATA_PACKET;
                else if (i == 0)
                    packetPositionFlag = SRTData.PositionFlags.FIRST;
                else if (i + MTU >= stream.Count)
                    packetPositionFlag = SRTData.PositionFlags.LAST;
                else
                    packetPositionFlag = SRTData.PositionFlags.MIDDLE;

                // Create the SRT packet header and payload
                srt_packet_data = new SRTData.SRTHeader(sequence_number: sequence_number, packetPositionFlag,
                    baseEncryption.Type == EncryptionType.None ? SRTData.EncryptionFlags.NOT_ENCRYPTED : SRTData.EncryptionFlags.ENCRYPTED,
                    is_retransmitted: retransmitted, message_number: messageNumber, dest_socket_id, packet_data);

                GetPayloadLayer() = OSIManager.BuildPLayer(srt_packet_data.GetByted(), true, baseEncryption);

                packets.Add(BuildPacket());  // Add the packet to the list of packets

                i += MTU;  // Move the index to the next packet
                messageNumber++;  // Increment the message number
            }
            return packets;
        }
    }
}
