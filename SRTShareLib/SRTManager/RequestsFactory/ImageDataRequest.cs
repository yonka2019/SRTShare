using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using SRTData = SRTShareLib.SRTManager.ProtocolFields.Data;


namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class ImageDataRequest : UdpPacket
    {
        public ImageDataRequest(params ILayer[] layers) : base(layers) { }

        public List<Packet> SplitToPackets(byte[] image, uint sequence_number, uint dest_socket_id, int MTU, BaseEncryption clientEncryption, bool retransmitted)
        {
            List<Packet> packets = new List<Packet>();
            byte[] packet_data;
            ushort imageChecksum = image.CalculateChecksum();  // ! calculating image checksum before any encryption !

            SRTData.SRTHeader srt_packet_data;
            SRTData.PositionFlags packetPositionFlag;

            int i = 0;
            uint messageNumber = 0;

            while (i < image.Length)  // Iterate over the image until all bytes in the stream has been processed
            {
                int packetLength = Math.Min(MTU, image.Length - i);  // Calculate the length of the packet to be sent (if the packet length smaller than the mtu, take it)
                packet_data = new ArraySegment<byte>(image, i, packetLength).ToArray();  // Get the packet data from the stream by the size) (same as List<>.GetRange)
                byte[] bPacket_data = packet_data.ToArray();

                if (clientEncryption.Type != EncryptionType.None)
                    packet_data = clientEncryption.Encrypt(bPacket_data);


                if (image.Length <= MTU)  // stream is not bigger than the MTU, there is no reason to split the image to chunks
                    packetPositionFlag = SRTData.PositionFlags.SINGLE_DATA_PACKET;
                else if (i == 0)
                    packetPositionFlag = SRTData.PositionFlags.FIRST;
                else if (i + MTU >= image.Length)
                    packetPositionFlag = SRTData.PositionFlags.LAST;
                else
                    packetPositionFlag = SRTData.PositionFlags.MIDDLE;

                // Create the SRT packet header and payload
                srt_packet_data = new SRTData.ImageData(sequence_number: sequence_number, packetPositionFlag,
                    clientEncryption.Type == EncryptionType.None ? SRTData.EncryptionFlags.NOT_ENCRYPTED : SRTData.EncryptionFlags.ENCRYPTED,
                    is_retransmitted: retransmitted, message_number: messageNumber, dest_socket_id, imageChecksum, packet_data);
                    
                GetPayloadLayer() = OSIManager.BuildPLayer(srt_packet_data.GetByted());

                packets.Add(BuildPacket());  // Add the packet to the list of packets

                i += MTU;  // Move the index to the next packet
                messageNumber++;  // Increment the message number
            }
            return packets;
        }
    }
}
