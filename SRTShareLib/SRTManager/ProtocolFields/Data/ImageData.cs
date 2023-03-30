using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public class ImageData : SRTHeader
    {
        public ImageData(PositionFlags packet_position_flag, EncryptionFlags encryption_flag, bool is_retransmitted, uint message_number, uint dest_socket_id, ushort imageChecksum, byte[] data) : base()
        {
            PACKET_POSITION_FLAG = (ushort)packet_position_flag; byteFields.Add(BitConverter.GetBytes(PACKET_POSITION_FLAG));
            ENCRYPTION_FLAG = Convert.ToBoolean(encryption_flag); byteFields.Add(BitConverter.GetBytes(ENCRYPTION_FLAG));

            RETRANSMITTED_PACKET_FLAG = is_retransmitted; byteFields.Add(BitConverter.GetBytes(RETRANSMITTED_PACKET_FLAG));

            MESSAGE_NUMBER = message_number; byteFields.Add(BitConverter.GetBytes(MESSAGE_NUMBER));
            DEST_SOCKET_ID = dest_socket_id; byteFields.Add(BitConverter.GetBytes(DEST_SOCKET_ID));

            IMAGE_CHECKSUM = imageChecksum; byteFields.Add(BitConverter.GetBytes(IMAGE_CHECKSUM));
            DATA = data; byteFields.Add(DATA);
        }

        public ImageData(byte[] payload) : base(payload)
        {
            PACKET_POSITION_FLAG = BitConverter.ToUInt16(payload, 5); // [5 6]
            ENCRYPTION_FLAG = BitConverter.ToBoolean(payload, 7); // [7]
            RETRANSMITTED_PACKET_FLAG = BitConverter.ToBoolean(payload, 8); // [8]

            MESSAGE_NUMBER = BitConverter.ToUInt32(payload, 9); // [9 10 11 12]
            DEST_SOCKET_ID = BitConverter.ToUInt32(payload, 13); // [13 14 15 16]

            IMAGE_CHECKSUM = BitConverter.ToUInt16(payload, 17);  // [17 18]

            // SIZES:              19     XXXX  --> PAYLOAD.LENGTH - 19 = DATA SIZE
            // PACKET PAYLOAD: [METADATA][DATA]
            DATA = new byte[payload.Length - 19];
            Array.Copy(payload, 19, DATA, 0, payload.Length - 19);  // [19 -> end]
        }

        /// <summary>
        /// 16 bits (2 bytes). The position of the packet in the whole message.
        /// </summary>
        public ushort PACKET_POSITION_FLAG { get; private set; }

        /// <summary>
        /// 8 bit (1 byte). true if encryption is used
        /// </summary>
        public bool ENCRYPTION_FLAG { get; private set; }

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
