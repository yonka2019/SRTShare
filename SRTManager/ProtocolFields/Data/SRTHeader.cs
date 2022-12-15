using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTManager.ProtocolFields.Data
{
    public class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }

        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        /// 

        public SRTHeader(uint sequence_number, PositionFlags packet_position_flag, EncryptionFlags encryption_flag, bool is_retransmitted, uint message_number, uint time_stamp, uint dest_socket_id, List<byte> data)
        {
            IS_CONTROL_PACKET = false; byteFields.Add(BitConverter.GetBytes(IS_CONTROL_PACKET));
            PACKET_POSITION_FLAG = (ushort)packet_position_flag; byteFields.Add(BitConverter.GetBytes(PACKET_POSITION_FLAG));
            KEY_BASED_ENCRYPTION_FLAG = (ushort)encryption_flag; byteFields.Add(BitConverter.GetBytes(KEY_BASED_ENCRYPTION_FLAG));
            RETRANSMITTED_PACKET_FLAG = is_retransmitted; byteFields.Add(BitConverter.GetBytes(RETRANSMITTED_PACKET_FLAG));

            MESSAGE_NUMBER = message_number; byteFields.Add(BitConverter.GetBytes(MESSAGE_NUMBER));
            TIMESTAMP = time_stamp; byteFields.Add(BitConverter.GetBytes(TIMESTAMP));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));
            Data = data; byteFields.Add(Data.ToArray());
        }

        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public SRTHeader(byte[] data)
        {
            //IS_CONTROL_PACKET = BitConverter.ToBoolean(data, 0); // [0]
            //CONTROL_TYPE = BitConverter.ToUInt16(data, 1); // [1 2]
            //SUB_TYPE = BitConverter.ToUInt16(data, 3); // [3 4]
            //TYPE_SPECIFIC_INFO = BitConverter.ToUInt32(data, 5); // [5 6 7 8]
            //DEST_SOCKET_ID = BitConverter.ToUInt32(data, 9); // [9 10 11 12]
        }

        public static bool IsData(byte[] data)
        {
            return !BitConverter.ToBoolean(data, 0);
        }

        /// <summary>
        /// 8 bit (1 bytes). The control packet has this flag set to
        /// "1". The data packet has this flag set to "0".
        /// </summary>
        public bool IS_CONTROL_PACKET { get; set; } // true (1) -> control packet | false (0) -> data packet


        public uint SEQUENCE_NUMBER { get; set; }

        public ushort PACKET_POSITION_FLAG { get; set; }

        public bool ORDER_FLAG { get; set; }

        public ushort KEY_BASED_ENCRYPTION_FLAG { get; set; }

        public bool RETRANSMITTED_PACKET_FLAG { get; set; }

        public uint MESSAGE_NUMBER { get; set; }

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

        public List<byte> Data { get; set; }
    }
}

