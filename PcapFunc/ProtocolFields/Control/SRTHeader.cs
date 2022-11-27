﻿using System;
using System.Collections.Generic;

namespace SRTManager.ProtocolFields.Control
{
    public class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }

        public SRTHeader(bool isData, ControlType packet_type, uint dest_socket_id, uint type_specific_info = 0)
        {
            IS_CONTROL_PACKET = isData; byteFields.Add(BitConverter.GetBytes(IS_CONTROL_PACKET));
            CONTROL_TYPE = (ushort)packet_type; byteFields.Add(BitConverter.GetBytes(CONTROL_TYPE));
            SUB_TYPE = 0x0; byteFields.Add(BitConverter.GetBytes(SUB_TYPE));
            TYPE_SPECIFIC_INFO = type_specific_info; byteFields.Add(BitConverter.GetBytes(TYPE_SPECIFIC_INFO));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));
        }

        /// <summary>
        /// 1 bit (=o bytes). The control packet has this flag set to
        /// "1". The data packet has this flag set to "0".
        /// </summary>
        public bool IS_CONTROL_PACKET { get; set; } // true (1) -> control packet | false (0) -> data packet

        /// <summary>
        /// 16 bits (2 bytes). Control Packet Type. The use of these bits
        /// is determined by the control packet type definition.
        /// </summary>
        public ushort CONTROL_TYPE { get; set; }

        /// <summary>
        /// 16 bits (2 bytes). This field specifies an additional subtype for
        /// specific packets.
        /// </summary>
        public ushort SUB_TYPE { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). The use of this field depends on
        /// the particular control packet type.Handshake packets do not use
        /// this field.
        /// </summary>
        public uint TYPE_SPECIFIC_INFO { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). The timestamp of the packet, in microseconds.
        /// The value is relative to the time the SRT connection was established.
        /// </summary>
        public uint TIMESTAMP { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). A fixed-width field providing the
        /// SRT socket ID to which a packet should be dispatched.The field
        /// may have the special value "0" when the packet is a connection request.
        /// </summary>
        public uint DEST_SOCKET_ID { get; set; }
    }
}
