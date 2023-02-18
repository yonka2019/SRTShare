using PcapDotNet.Packets;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.EManager.Encryption;
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
        public Packet Alive(uint dest_socket_id, bool videoStage = false, EncryptionType encryptionType = EncryptionType.None)
        {
            GetPayloadLayer() = OSIManager.BuildPLayer(new SRTControl.KeepAlive(dest_socket_id).GetByted(), videoStage, encryptionType, GetLayers());
            return BuildPacket();
        }
    }
}
