using SRTShareLib.SRTManager.ProtocolFields.Control;
using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public class AudioData : SRTHeader
    {
        public AudioData(uint sequence_number, PositionFlags packet_position_flag, EncryptionFlags encryption_flag, byte[] data) : base(DataType.AUDIO, sequence_number, packet_position_flag, encryption_flag)
        {
            DATA = data; byteFields.Add(DATA);
        }

        public AudioData(byte[] payload) : base(payload)
        {
            // SIZES:              10     XXXX  --> PAYLOAD.LENGTH - 10 = DATA SIZE
            // PACKET PAYLOAD: [METADATA][DATA]
            DATA = new byte[payload.Length - 10];
            Array.Copy(payload, 10, DATA, 0, payload.Length - 10);  // [10 -> end]
        }

        /// <summary>
        /// The function checks if it's a handshake packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if handshake, false if not</returns>
        public static bool IsAudio(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)DataType.AUDIO;
        }

        /// <summary>
        /// The actual data of the packet.
        /// </summary>
        public byte[] DATA { get; set; }
    }
}
