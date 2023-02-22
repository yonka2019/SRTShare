﻿using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTControl = SRTShareLib.SRTManager.ProtocolFields.Control;


namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class QualityUpdateRequest : UdpPacket
    {
        public QualityUpdateRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a quality control packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A quality control packet</returns>
        public Packet UpdateQuality(uint dest_socket_id = 0, long newQuality = ProtocolManager.DEFAULT_QUALITY, bool videoStage = false, PeerEncryption peerEncryption = default)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new SRTControl.QualityUpdate(dest_socket_id, newQuality).GetByted(), videoStage, peerEncryption);
            return BuildPacket();
        }
    }
}
