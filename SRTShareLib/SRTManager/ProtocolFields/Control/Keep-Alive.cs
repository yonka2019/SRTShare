﻿using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class KeepAlive : SRTHeader
    {
        public KeepAlive(uint dest_socket_id, uint source_socket_id) : base(ControlType.KEEPALIVE, dest_socket_id, source_socket_id)
        {
        }

        public KeepAlive(byte[] data) : base(data)  // initialize SRT Control header fields
        {
        }


        /// <summary>
        /// The function checks if it's a keep alive packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if keep alive, false if not</returns>
        public static bool IsKeepAlive(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.KEEPALIVE;
        }
    }
}
