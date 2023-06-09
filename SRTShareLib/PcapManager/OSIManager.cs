﻿using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Collections.Generic;
using System.Linq;

namespace SRTShareLib.PcapManager
{
    public class OSIManager
    {
        /// <summary>
        /// The function builds the ethernet layer
        /// </summary>
        /// <param name="sourceMac">Source mac</param>
        /// <param name="destMac">Destination mac</param>
        /// <returns>Ethernet layer object</returns>
        public static EthernetLayer BuildEthernetLayer(string sourceMac, string destMac)
        {
            return
            new EthernetLayer
            {
                Source = new MacAddress(sourceMac),
                Destination = new MacAddress(destMac),
                EtherType = EthernetType.None, // Will be filled automatically.
            };
        }

        /// <summary>
        /// The function builds the ip layer
        /// </summary>
        /// <param name="sourceIp">Source ip</param>
        /// <param name="destIp">Destination ip</param>
        /// <returns>Ip layer object</returns>
        public static IpV4Layer BuildIpv4Layer(string sourceIp, string destIp)
        {
            return
            new IpV4Layer
            {
                Source = new IpV4Address(sourceIp),
                CurrentDestination = new IpV4Address(destIp),
                Fragmentation = IpV4Fragmentation.None,
                HeaderChecksum = null, // Will be filled automatically.
                Identification = 123,
                Options = IpV4Options.None,
                Protocol = null, // Will be filled automatically.
                Ttl = 100,
                TypeOfService = 0,
            };
        }

        /// <summary>
        /// The function builds the transport layer
        /// </summary>
        /// <param name="sourcePort">Source port</param>
        /// <param name="destPort">Destination port</param>
        /// <returns>Transport layer object (udp)</returns>
        public static UdpLayer BuildUdpLayer(ushort sourcePort, ushort destPort)
        {
            return
            new UdpLayer
            {
                SourcePort = sourcePort,
                DestinationPort = destPort,
                Checksum = null, // Will be filled automatically.
                CalculateChecksumValue = true,
            };
        }

        #region Build Payload Layer functions

        #region func ConcatBytes() - https://stackoverflow.com/questions/4875968/concatenating-a-c-sharp-list-of-byte
        /// <summary>
        /// Concat all list of bytes[] into one big byte[] array
        /// </summary>
        /// <param name="data">list of bytes[]</param>
        /// <returns>big byte[] array</returns>
        private static byte[] ConcatBytes(List<byte[]> data)
        {
            byte[] output = new byte[data.Sum(arr => arr.Length)];
            int writeIdx = 0;

            foreach (byte[] byteArr in data)
            {
                byteArr.CopyTo(output, writeIdx);
                writeIdx += byteArr.Length;
            }
            return output;
        }
        #endregion

        /// <summary>
        /// The function converts a byte list into a payload layer
        /// </summary>
        /// <param name="data">Data to convert</param>
        /// <returns>Payload layer object</returns>
        public static PayloadLayer BuildPLayer(List<byte[]> data)
        {
            byte[] bytedData = ConcatBytes(data);
            return BuildPLayer(bytedData);
        }

        /// <summary>
        /// The function converts a byte array into a payload layer
        /// </summary>
        /// <param name="data">Data to convert</param>
        /// <returns>Payload layer object</returns>
        private static PayloadLayer BuildPLayer(byte[] data)
        {
            return new PayloadLayer
            {
                Data = new Datagram(data)
            };
        }
        #endregion

        /// <summary>
        /// The function builds all of the base layers (ehternet, ip, transport)
        /// </summary>
        /// <param name="sourcePort">Source port</param>
        /// <param name="destPort">Destination port</param>
        /// <returns>List of the base layers</returns>
        public static ILayer[] BuildBaseLayers(string sourceMac, string destMac, string sourceIp, string destIp, ushort sourcePort, ushort destPort)
        {
            ILayer[] baseLayers = new ILayer[3];

            baseLayers[0] = BuildEthernetLayer(sourceMac, destMac);
            baseLayers[1] = BuildIpv4Layer(sourceIp, destIp);
            baseLayers[2] = BuildUdpLayer(sourcePort, destPort);

            return baseLayers;
        }
    }
}
