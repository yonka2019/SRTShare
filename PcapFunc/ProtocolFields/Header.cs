using System.Collections.Generic;
using System;

namespace SRTManager.ProtocolFields
{
    public class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }


        public SRTHeader(SRTManager.ProtocolFields.PacketType packet_type, uint dest_socket_id, uint type_specific_info = 0)
        {
            //byteFields.Add(BitConverter.GetBytes(1)); // change to 1 bit 
            CONTROL_TYPE = (ushort)packet_type; byteFields.Add(BitConverter.GetBytes(CONTROL_TYPE)); // transfer to 15 bits (instead of 16)

            SUB_TYPE = 0x0; byteFields.Add(BitConverter.GetBytes(SUB_TYPE));
            TYPE_SPECIFIC_INFO = type_specific_info; byteFields.Add(BitConverter.GetBytes(TYPE_SPECIFIC_INFO));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));


        /// <summary>
        /// 15 bits (2 bytes). Declares what is the control's type.
        /// </summary>
        public ushort CONTROL_TYPE { get; set; }

        /// <summary>
        /// 16 bits (2 bytes). Declares what is the control's sub type.
        /// </summary>
        public ushort SUB_TYPE { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). Depends on the particular control packet type.
        /// </summary>
        public uint TYPE_SPECIFIC_INFO;  // Change later (no use in HandShake)

        /// <summary>
        /// 32 bits (4 bytes). This field holds the ID of the dest SRT
        /// </summary>
        public uint DEST_SOCKET_ID { get; set; }

        /// <summary>
        /// 32 bits (4 bytes).
        /// </summary>
        //public uint CONTROL_INFO_FIELD { get; set; }

        

    }
}
