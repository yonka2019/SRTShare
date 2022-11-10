using PcapDotNet.Packets;
using System;

namespace SRTManager
{
    public class ProtocolManager
    {
        private ILayer[] workingLayers; // layers we working with, we should add last packet data (SRT)
        /*
         * EthernetLayer - Exist
         * InternetLayer - Exist
         * UDPLayer - Exist
         * PayloadLayer (SRT Data) - Should be added
         */

        public ProtocolManager(params ILayer[] layers)
        {
            workingLayers = new ILayer[layers.Length + 1]; // add payload layer after
            layers.CopyTo(workingLayers, 0);
        }

        public Packet HandshakeRequest() // add data to working packet
        {
            PayloadLayer pLayer = PacketManager.BuildPLayer("lala");
            workingLayers[workingLayers.Length - 1] = pLayer;

            return new PacketBuilder(workingLayers).Build(DateTime.Now);
        }
    }
}