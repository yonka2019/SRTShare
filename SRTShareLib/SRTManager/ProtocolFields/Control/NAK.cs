﻿using System;
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
        public NAK(uint dest_socket_id, List<uint> lost_packets) : base(ControlType.NAK, dest_socket_id)
        {
            LOST_PACKETS = lost_packets;
            foreach (uint value in LOST_PACKETS)
            {
                byteFields.Add(BitConverter.GetBytes(value));
            }
        }


        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public NAK(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            LOST_PACKETS = new List<uint>();

            for (int i = 13; i < data.Length; i += 4) // [13 -> end]
            {
                uint value = BitConverter.ToUInt32(data.ToArray(), i);
                LOST_PACKETS.Add(value);
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
        /// list of 32 bits (2 bytes). All the lost packets' sequence numbers
        /// </summary>
        public List<uint> LOST_PACKETS { get; private set; }
    }
}
