﻿using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using SRTData = SRTShareLib.SRTManager.ProtocolFields.Data;


namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class AudioDataRequest : UdpPacket
    {
        public AudioDataRequest(params ILayer[] layers) : base(layers) { }

        public List<Packet> SplitToPackets(byte[] audio, uint sequence_number, int MTU, BaseEncryption clientEncryption)
        {
            List<Packet> packets = new List<Packet>();
            byte[] packet_data;

            SRTData.SRTHeader srt_packet_data;
            SRTData.PositionFlags packetPositionFlag;

            int i = 0;

            while (i < audio.Length)  // Iterate over the image until all bytes in the stream has been processed
            {
                int packetLength = Math.Min(MTU, audio.Length - i);  // Calculate the length of the packet to be sent (if the packet length smaller than the mtu, take it)
                packet_data = new ArraySegment<byte>(audio, i, packetLength).ToArray();  // Get the packet data from the stream by the size) (same as List<>.GetRange)
                byte[] bPacket_data = packet_data.ToArray();

                if (clientEncryption.Type != EncryptionType.None)
                    packet_data = clientEncryption.Encrypt(bPacket_data);


                if (audio.Length <= MTU)  // stream is not bigger than the MTU, there is no reason to split the image to chunks
                    packetPositionFlag = SRTData.PositionFlags.SINGLE_DATA_PACKET;
                else if (i == 0)
                    packetPositionFlag = SRTData.PositionFlags.FIRST;
                else if (i + MTU >= audio.Length)
                    packetPositionFlag = SRTData.PositionFlags.LAST;
                else
                    packetPositionFlag = SRTData.PositionFlags.MIDDLE;

                // Create the SRT packet header and payload
                srt_packet_data = new SRTData.AudioData(sequence_number: sequence_number, packetPositionFlag,
                    clientEncryption.Type == EncryptionType.None ? SRTData.EncryptionFlags.NOT_ENCRYPTED : SRTData.EncryptionFlags.ENCRYPTED, packet_data);

                GetPayloadLayer() = OSIManager.BuildPLayer(srt_packet_data.GetByted());

                packets.Add(BuildPacket());  // Add the packet to the list of packets

                i += MTU;  // Move the index to the next packet
            }
            return packets;
        }
    }
}
