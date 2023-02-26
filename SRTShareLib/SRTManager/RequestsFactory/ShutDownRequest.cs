using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTControl = SRTShareLib.SRTManager.ProtocolFields.Control;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class ShutdownRequest : UdpPacket
    {
        public ShutdownRequest(params ILayer[] layers) : base(layers) { }

        /// <summary>
        /// The function creates a shutdown packet
        /// </summary>
        /// <param name="dest_socket_id">Destination socket id</param>
        /// <returns>A shutdown packet</returns>
        public Packet Shutdown(uint dest_socket_id, uint source_socket_id, bool videoStage = false, BaseEncryption baseEncryption = null)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new SRTControl.Shutdown(dest_socket_id, source_socket_id).GetByted(), videoStage, baseEncryption);
            return BuildPacket();
        }
    }
}
