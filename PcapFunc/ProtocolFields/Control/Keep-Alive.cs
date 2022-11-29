using System;

namespace SRTManager.ProtocolFields.Control
{
    public class KeepAlive : SRTHeader
    {
        public KeepAlive(uint dest_socket_id) : base(ControlType.KEEPALIVE, dest_socket_id)
        {
        }

        public KeepAlive(byte[] data) : base(data)  // initialize SRT Control header fields
        {
        }

        public static bool IsKeepAlive(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.KEEPALIVE;
        }
    }
}
