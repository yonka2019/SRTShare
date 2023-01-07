﻿using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using SRTData = SRTLibrary.SRTManager.ProtocolFields.Data;


namespace SRTLibrary.SRTManager.RequestsFactory
{
    public class DataRequest : UdpPacket
    {
        public DataRequest(params ILayer[] layers) : base(layers) { }

        public  List<Packet> SplitToPackets(List<byte> stream, uint time_stamp, uint dest_socket_id, int MTU)
        {
            List<Packet> packets = new List<Packet>();
            List<byte> packet_data;
            SRTData.SRTHeader srt_packet_data;

            int i = 0;
            uint messageNumber = 0;

            while (i < stream.Count)  // Iterate until all bytes in the stream have been processed
            {
                int packetLength = Math.Min(MTU, stream.Count - i);  // Calculate the length of the packet to be sent
                packet_data = stream.GetRange(i, packetLength);  // Get the packet data from the stream

                SRTData.PositionFlags packetPositionFlag;
                if (i == 0)
                {
                    packetPositionFlag = SRTData.PositionFlags.FIRST;
                }
                else if (i + MTU >= stream.Count)
                {
                    packetPositionFlag = SRTData.PositionFlags.LAST;
                }
                else
                {
                    packetPositionFlag = SRTData.PositionFlags.MIDDLE;
                }

                // Create the SRT packet header and payload
                srt_packet_data = new SRTData.SRTHeader(sequence_number: 0, packetPositionFlag, SRTData.EncryptionFlags.NOT_ENCRYPTED, is_retransmitted: false, message_number: messageNumber, time_stamp, dest_socket_id, packet_data);
                GetPayloadLayer() = PacketManager.BuildPLayer(srt_packet_data.GetByted());

                packets.Add(BuildPacket());  // Add the packet to the list of packets

                i += MTU;  // Move the index to the next packet
                messageNumber++;  // Increment the message number
            }

            return packets;
        }
    }
}
