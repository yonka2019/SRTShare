﻿using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTControl = SRTShareLib.SRTManager.ProtocolFields.Control;


namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class KeepAliveRequest : UdpPacket
    {
        public KeepAliveRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a keep alive packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A keep alive packet</returns>
        public Packet Alive(uint dest_socket_id, uint source_socket_id)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new SRTControl.KeepAlive(dest_socket_id, source_socket_id).GetByted());
            return BuildPacket();
        }
    }
}
