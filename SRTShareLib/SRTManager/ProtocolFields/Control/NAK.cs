using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class NAK : SRTHeader
    {
        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public NAK(uint dest_socket_id, uint source_socket_id, uint corrupted_sequence_number, List<uint> lost_packets) : base(ControlType.NAK, dest_socket_id, source_socket_id)
        {
            CORRUPTED_SEQUENCE_NUMBER = corrupted_sequence_number; byteFields.Add(BitConverter.GetBytes(CORRUPTED_SEQUENCE_NUMBER));
            MESSAGE_NUMBER_LOST_PACKETS = lost_packets;

            foreach (uint value in MESSAGE_NUMBER_LOST_PACKETS)
            {
                byteFields.Add(BitConverter.GetBytes(value));
            }
        }


        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public NAK(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            CORRUPTED_SEQUENCE_NUMBER = BitConverter.ToUInt32(data, 11);  // [11 12 13 14]

            MESSAGE_NUMBER_LOST_PACKETS = new List<uint>();

            for (int i = 15; i < data.Length; i += 4) // [15 -> end]
            {
                uint value = BitConverter.ToUInt32(data.ToArray(), i);
                MESSAGE_NUMBER_LOST_PACKETS.Add(value);
            }
        }


        /// <summary>
        /// The function checks if it's a nak packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if nak, false if not</returns>
        public static bool IsNAK(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.NAK;
        }

        /// <summary>
        /// 32 bits (2 bytes). number of the corrupted sequence number where the chunks got lost
        /// </summary>
        public uint CORRUPTED_SEQUENCE_NUMBER { get; private set; }

        /// <summary>
        /// 32 bits (2 bytes). list of All the lost packets' message numbers
        /// </summary>
        public List<uint> MESSAGE_NUMBER_LOST_PACKETS { get; private set; }

    }
}
