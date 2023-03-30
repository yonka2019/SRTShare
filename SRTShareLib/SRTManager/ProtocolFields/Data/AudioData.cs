using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public class AudioData : SRTHeader
    {
        public AudioData(uint sequence_number, PositionFlags packet_position_flag, EncryptionFlags encryption_flag, bool is_retransmitted, uint message_number, uint dest_socket_id, ushort imageChecksum, byte[] data) : base(DataType.AUDIO, sequence_number)
        {

        }

        public AudioData(byte[] payload) : base(payload)
        {
            PACKET_POSITION_FLAG = BitConverter.ToUInt16(payload, 7); // [7 8]
            ENCRYPTION_FLAG = BitConverter.ToBoolean(payload, 9); // [9]
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
        /// The actual data of the packet.
        /// </summary>
        public byte[] DATA { get; set; }
    }
}
