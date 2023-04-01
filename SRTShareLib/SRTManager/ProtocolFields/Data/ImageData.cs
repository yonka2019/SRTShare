using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public class ImageData : SRTHeader
    {
        public ImageData(uint sequence_number, PositionFlags packet_position_flag, EncryptionFlags encryption_flag, bool is_retransmitted, uint message_number, uint dest_socket_id, ushort imageChecksum, byte[] data) : base(DataType.IMAGE, sequence_number, packet_position_flag, encryption_flag)
        {
            RETRANSMITTED_PACKET_FLAG = is_retransmitted; byteFields.Add(BitConverter.GetBytes(RETRANSMITTED_PACKET_FLAG));
            MESSAGE_NUMBER = message_number; byteFields.Add(BitConverter.GetBytes(MESSAGE_NUMBER));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));

            IMAGE_CHECKSUM = imageChecksum; byteFields.Add(BitConverter.GetBytes(IMAGE_CHECKSUM));
            DATA = data; byteFields.Add(DATA);
        }

        public ImageData(byte[] payload) : base(payload)
        {
            RETRANSMITTED_PACKET_FLAG = BitConverter.ToBoolean(payload, 10); // [10]
            MESSAGE_NUMBER = BitConverter.ToUInt32(payload, 11); // [11 12 13 14]
            DEST_SOCKET_ID = BitConverter.ToUInt32(payload, 15); // [15 16 17 18]

            IMAGE_CHECKSUM = BitConverter.ToUInt16(payload, 19);  // [19 20]

            // SIZES:              21     XXXX  --> PAYLOAD.LENGTH - 21 = DATA SIZE
            // PACKET PAYLOAD: [METADATA][DATA]
            DATA = new byte[payload.Length - 21];
            Array.Copy(payload, 21, DATA, 0, payload.Length - 21);  // [21 -> end]
        }

        /// <summary>
        /// The function checks if it's a handshake packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if handshake, false if not</returns>
        public static bool IsImage(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)DataType.IMAGE;
        }

        /// <summary>
        /// 8 bits (1 byte). True if the packet is retransmitted (was sent more than once). False if not.
        /// </summary>
        public bool RETRANSMITTED_PACKET_FLAG { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). The sequential number of consecutive data packets that form a message
        /// </summary>
        public uint MESSAGE_NUMBER { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). A fixed-width field providing the
        /// SRT socket ID to which a packet should be dispatched.The field
        /// may have the special value "0" when the packet is a connection request.
        /// </summary>
        public uint DEST_SOCKET_ID { get; private set; }

        /// <summary>
        /// 16 bits (2 bytes). whole image checksum which is calculated via internet checksum method in order to identify corrupted data
        /// (also sensitive in a case if the bytes order changed ==> ORDER DEPENDED)
        /// The checksum of the ALL chunks (of same image) is the SAME, it's the checksum of the WHOLE image (all chunks concated => image)
        /// </summary>
        public ushort IMAGE_CHECKSUM { get; private set; }

        /// <summary>
        /// The actual data of the packet.
        /// </summary>
        public byte[] DATA { get; set; }
    }
}
