﻿using PcapDotNet.Packets;
using System;

namespace SRTManager.RequestsFactory
{
    public class UdpPacket
    {
        /* Usage Example:
            EthernetLayer ethernetLayer = PacketManager.BuildEthernetLayer();
            IpV4Layer ipV4Layer = PacketManager.BuildIpv4Layer();
            UdpLayer udpLayer = PacketManager.BuildUdpLayer(PacketManager.SERVER_PORT, 123);
            var a = new ProtocolManager(ethernetLayer, ipV4Layer, udpLayer);
            Packet doneToSend = a.HandshakeRequest();
         */

        private readonly ILayer[] workingLayers; // layers we working with, we should add last packet data (SRT)
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

        protected ref ILayer GetPayloadLayer()
        {
            return ref workingLayers[workingLayers.Length - 1];
        }

        protected Packet BuildPacket()
        {
            return new PacketBuilder(workingLayers).Build(DateTime.Now);
        }
    }
}