using System;
using System.Collections.Generic;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }

        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public SRTHeader(uint sequence_number, PositionFlags packet_position_flag, EncryptionFlags encryption_flag, bool is_retransmitted, uint message_number, uint time_stamp, uint dest_socket_id, List<byte> data)
        {
            IS_CONTROL_PACKET = false; byteFields.Add(BitConverter.GetBytes(IS_CONTROL_PACKET));
            SEQUENCE_NUMBER = sequence_number; byteFields.Add(BitConverter.GetBytes(SEQUENCE_NUMBER));

            PACKET_POSITION_FLAG = (ushort)packet_position_flag; byteFields.Add(BitConverter.GetBytes(PACKET_POSITION_FLAG));
            ORDER_FLAG = false; byteFields.Add(BitConverter.GetBytes(ORDER_FLAG));
            KEY_BASED_ENCRYPTION_FLAG = (ushort)encryption_flag; byteFields.Add(BitConverter.GetBytes(KEY_BASED_ENCRYPTION_FLAG));
            RETRANSMITTED_PACKET_FLAG = is_retransmitted; byteFields.Add(BitConverter.GetBytes(RETRANSMITTED_PACKET_FLAG));

            MESSAGE_NUMBER = message_number; byteFields.Add(BitConverter.GetBytes(MESSAGE_NUMBER));
            TIMESTAMP = time_stamp; byteFields.Add(BitConverter.GetBytes(TIMESTAMP));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));
            DATA = data; byteFields.Add(DATA.ToArray());
        }

        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public SRTHeader(byte[] data)
        {
            IS_CONTROL_PACKET = BitConverter.ToBoolean(data, 0); // [0]
            SEQUENCE_NUMBER = BitConverter.ToUInt32(data, 1); // [1 2 3 4]

            PACKET_POSITION_FLAG = BitConverter.ToUInt16(data, 5); // [5 6]
            ORDER_FLAG = false; BitConverter.ToBoolean(data, 7); // [7]
            KEY_BASED_ENCRYPTION_FLAG = BitConverter.ToUInt16(data, 8); // [8 9]
            RETRANSMITTED_PACKET_FLAG = BitConverter.ToBoolean(data, 10); // [10]

            MESSAGE_NUMBER = BitConverter.ToUInt32(data, 11); // [11 12 13 14]
            TIMESTAMP = BitConverter.ToUInt32(data, 15); // [15 16 17 18]
            DEST_SOCKET_ID = BitConverter.ToUInt32(data, 19); // [19 20 21 22]

            DATA = new List<byte>();
            for (int i = 23; i < data.Length; i++) // [23 -> end]
            {
                DATA.Add(data[i]);
            }
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
        /// 8 bit (1 bytes). The control packet has this flag set to
        /// "1". The data packet has this flag set to "0".
        /// </summary>
        public bool IS_CONTROL_PACKET { get; private set; } // true (1) -> control packet | false (0) -> data packet

        /// <summary>
        /// 32 bits (4 bytes). The sequence number field.
        /// </summary>
        public uint SEQUENCE_NUMBER { get; private set; }

        /// <summary>
        /// 16 bits (2 bytes). The position of the packet in the whole message.
        /// </summary>
        public ushort PACKET_POSITION_FLAG { get; private set; }

        /// <summary>
        /// 8 bits (1 byte). True if the packets need to be in order. False if not.
        /// </summary>
        public bool ORDER_FLAG { get; private set; }

        /// <summary>
        /// 16 bits (2 bytes). Different flags for each encryption option. 
        /// </summary>
        public ushort KEY_BASED_ENCRYPTION_FLAG { get; private set; }

        /// <summary>
        /// 8 bits (1 byte). True if the packet is retransmitted (was sent more than once). False if not.
        /// </summary>
        public bool RETRANSMITTED_PACKET_FLAG { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). The sequential number of consecutive data packets that form a message
        /// </summary>
        public uint MESSAGE_NUMBER { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). The timestamp of the packet, in microseconds.
        /// The value is relative to the time the SRT connection was established.
        /// </summary>
        public uint TIMESTAMP { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). A fixed-width field providing the
        /// SRT socket ID to which a packet should be dispatched.The field
        /// may have the special value "0" when the packet is a connection request.
        /// </summary>
        public uint DEST_SOCKET_ID { get; private set; }

        /// <summary>
        /// The actual data of the packet.
        /// </summary>
        public List<byte> DATA { get; private set; }
    }
}

