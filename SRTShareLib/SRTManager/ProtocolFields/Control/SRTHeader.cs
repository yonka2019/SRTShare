﻿using System;
using System.Collections.Generic;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public abstract class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }

        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public SRTHeader(ControlType packet_type, uint dest_socket_id, uint source_socket_id)
        {
            IS_CONTROL_PACKET = true; byteFields.Add(BitConverter.GetBytes(IS_CONTROL_PACKET));
            CONTROL_TYPE = (ushort)packet_type; byteFields.Add(BitConverter.GetBytes(CONTROL_TYPE));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));
            SOURCE_SOCKET_ID = source_socket_id; byteFields.Add(BitConverter.GetBytes(SOURCE_SOCKET_ID));
        }

        /// <summary>
        /// Byte[] -> Fields (To extract) [0 -> 10]
        /// </summary>
        public SRTHeader(byte[] data)
        {
            IS_CONTROL_PACKET = BitConverter.ToBoolean(data, 0); // [0] (1 byte)
            CONTROL_TYPE = BitConverter.ToUInt16(data, 1); // [1 2] (2 bytes)
            DEST_SOCKET_ID = BitConverter.ToUInt32(data, 3); // [3 4 5 6] (4 bytes)
            SOURCE_SOCKET_ID = BitConverter.ToUInt32(data, 7); // [7 8 9 10] (4 bytes)
        }

        /// <summary>
        /// The function checks if it's a control packet
        /// </summary>
        /// <param name="data">Data to check</param>
        /// <returns>True if control packet, false if not</returns>
        public static bool IsControl(byte[] data)
        {
            return BitConverter.ToBoolean(data, 0);
        }

        /// <summary>
        /// 8 bit (1 bytes). The control packet has this flag set to
        /// "1". The data packet has this flag set to "0".
        /// </summary>
        public bool IS_CONTROL_PACKET { get; private set; }  // true (1) -> control packet | false (0) -> data packet

        /// <summary>
        /// 16 bits (2 bytes). Control Packet Type. The use of these bits
        /// is determined by the control packet type definition.
        /// </summary>
        public ushort CONTROL_TYPE { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). A fixed-width field providing the
        /// Destination SRT socket ID to which a packet should be dispatched. The field
        /// may have the special value "0" when the packet is a connection request.
        /// </summary>
        public uint DEST_SOCKET_ID { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). A fixed-width field providing the
        /// Source SRT socket ID to which a packet should be dispatched. The field
        /// may have the special value "0" when the packet is a connection request.
        /// </summary>
        public uint SOURCE_SOCKET_ID { get; private set; }
    }
}
