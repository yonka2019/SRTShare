using PcapDotNet.Packets;
using System;

namespace SRTShareLib.SRTManager.RequestsFactory
{
    public class UdpPacket
    {
        private readonly ILayer[] workingLayers;  // layers we working with, we should add last packet data (SRT)
        /*
         * EthernetLayer - Exist
         * InternetLayer - Exist
         * UDPLayer - Exist
         * PayloadLayer (SRT Data) - Should be added
         */

        public UdpPacket(params ILayer[] layers)
        {
            workingLayers = new ILayer[layers.Length + 1]; // add payload layer after
            layers.CopyTo(workingLayers, 0);
        }

        /// <summary>
        /// The function returns the payload layer
        /// </summary>
        /// <returns>The reference to the payload layer</returns>
        protected ref ILayer GetPayloadLayer()
        {
            return ref workingLayers[workingLayers.Length - 1];
        }

        /// <summary>
        /// The function builds and returns a packet based on the existing layers
        /// </summary>
        /// <returns>Packet object</returns>
        protected Packet BuildPacket()
        {
            return new PacketBuilder(workingLayers).Build(DateTime.MinValue);
        }
    }
}