using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class Shutdown : SRTHeader
    {
        public Shutdown(uint dest_socket_id, uint source_socket_id) : base(ControlType.SHUTDOWN, dest_socket_id, source_socket_id)
        {
        }

        public Shutdown(byte[] data) : base(data)  // initialize SRT Control header fields
        {
        }


        /// <summary>
        /// The function checks if it's a shutdown packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if shutdown, false if not</returns>
        public static bool IsShutdown(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.SHUTDOWN;
        }
    }
}
