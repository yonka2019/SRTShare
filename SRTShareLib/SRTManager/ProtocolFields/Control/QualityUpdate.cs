using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class QualityUpdate : SRTHeader
    {
        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public QualityUpdate(uint dest_socket_id, long quality) : base(ControlType.QUALITY_UPDATE, dest_socket_id)
        {
            QUALITY = quality; byteFields.Add(BitConverter.GetBytes(QUALITY));
        }

        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public QualityUpdate(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            QUALITY = data[13];  // [13]
        }


        /// <summary>
        /// The function checks if it's a quality control packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if keep alive, false if not</returns>
        public static bool IsQualityUpdate(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.QUALITY_UPDATE;
        }

        /// <summary>
        /// 8 bits (1 byte). Set new quality to the data at the [server - client] conversation
        /// </summary>
        public long QUALITY { get; private set; }
    }
}
