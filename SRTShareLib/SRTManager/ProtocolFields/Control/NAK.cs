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
        public NAK(uint dest_socket_id, uint source_socket_id, uint corrupted_sequence_number) : base(ControlType.NAK, dest_socket_id, source_socket_id)
        {
            CORRUPTED_SEQUENCE_NUMBER = corrupted_sequence_number; byteFields.Add(BitConverter.GetBytes(CORRUPTED_SEQUENCE_NUMBER));
        }


        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public NAK(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            CORRUPTED_SEQUENCE_NUMBER = BitConverter.ToUInt32(data, 11);  // [11 12 13 14]
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
        /// 32 bits (4 bytes). number of the corrupted sequence number where the chunks got lost
        /// </summary>
        public uint CORRUPTED_SEQUENCE_NUMBER { get; private set; }
    }
}
