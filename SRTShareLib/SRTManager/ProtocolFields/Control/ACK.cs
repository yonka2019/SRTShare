﻿using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class ACK : SRTHeader
    {
        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public ACK(uint dest_socket_id, uint source_socket_id, uint ack_sequence_number) : base(ControlType.ACK, dest_socket_id, source_socket_id)
        {
            ACK_SEQUENCE_NUMBER = ack_sequence_number; byteFields.Add(BitConverter.GetBytes(ACK_SEQUENCE_NUMBER));
        }


        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public ACK(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            ACK_SEQUENCE_NUMBER = BitConverter.ToUInt32(data, 11); // [11 12 13 14]
        }


        /// <summary>
        /// The function checks if it's a ack packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if ack, false if not</returns>
        public static bool IsACK(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.ACK;
        }


        /// <summary>
        /// 32 bits (2 bytes). The sequnce number of the successfully received packet (sequence number / image)
        /// </summary>
        public uint ACK_SEQUENCE_NUMBER { get; private set; }
    }
}
