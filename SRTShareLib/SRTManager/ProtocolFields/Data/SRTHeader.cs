using System;
using System.Collections.Generic;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public abstract class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }

        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public SRTHeader(DataType dataType, uint sequence_number)
        {
            IS_CONTROL_PACKET = false; byteFields.Add(BitConverter.GetBytes(IS_CONTROL_PACKET));
            DATA_TYPE = (ushort)dataType; byteFields.Add(BitConverter.GetBytes(DATA_TYPE));
            SEQUENCE_NUMBER = sequence_number; byteFields.Add(BitConverter.GetBytes(SEQUENCE_NUMBER));
        }

        /// <summary>
        /// Byte[] -> Fields (To extract) [0 -> 6]
        /// </summary>
        public SRTHeader(byte[] payload)
        {
            IS_CONTROL_PACKET = BitConverter.ToBoolean(payload, 0); // [0]
            DATA_TYPE = BitConverter.ToUInt16(payload, 1);  // [1 2]
            SEQUENCE_NUMBER = BitConverter.ToUInt32(payload, 3); // [3 4 5 6]
        }

        /// <summary>
        /// The function checks if it's a data packet
        /// </summary>
        /// <param name="data">Data to check</param>
        /// <returns>True if data packet, false if not</returns>
        public static bool IsData(byte[] data)
        {
            return !BitConverter.ToBoolean(data, 0);
        }

        /// <summary>
        /// 8 bit (1 byte). The control packet has this flag set to
        /// "1". The data packet has this flag set to "0".
        /// </summary>
        public bool IS_CONTROL_PACKET { get; private set; }  // true (1) -> control packet | false (0) -> data packet

        /// <summary>
        /// 16 bits (2 bytes). Data Packet Type. The use of these bits
        /// is determined by the data packet type definition.
        /// </summary>
        public ushort DATA_TYPE { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). The sequence number field.
        /// </summary>
        public uint SEQUENCE_NUMBER { get; private set; }
    }
}

