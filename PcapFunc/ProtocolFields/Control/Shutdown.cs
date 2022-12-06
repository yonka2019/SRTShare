using System;

namespace SRTManager.ProtocolFields.Control
{
    public class Shutdown : SRTHeader
    {
        public Shutdown(uint dest_socket_id) : base(ControlType.SHUTDOWN, dest_socket_id)
        {
        }

        public Shutdown(byte[] data) : base(data)  // initialize SRT Control header fields
        {
        }

        public static bool IsShutdown(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.SHUTDOWN;
        }
    }
}
