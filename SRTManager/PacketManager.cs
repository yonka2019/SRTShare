using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRTManager
{
    public static class PacketManager
    {
        public static readonly PacketDevice pcapDevice;

        public const int SERVER_PORT = 6969;
        public static readonly SAddress LOOPBACK_IP = new SAddress("127.0.0.1");
        static PacketManager()
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int deviceIndex = -1;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("[ERROR] NO INTERFACES FOUND");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // iterate interfaces list and found the right one
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                if (device.Description != null)
                {
                    if (!device.Description.ToUpper().Contains("VIRTUAL") && !device.Description.ToUpper().Contains("LOOPBACK") && !device.Description.ToUpper().Contains("MICROSOFT"))  // not virtual & not loopback & not microsoft
                    {
                        deviceIndex = i + 1;
                        break;
                    }
                }
            }

            if (deviceIndex == -1)
            {
                Console.WriteLine($"[ERROR] THERE IS NO INTERFACE WHICH MET THE REQUIREMENTS");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Take the selected adapter
            pcapDevice = allDevices[deviceIndex - 1];
            Console.WriteLine($"[!] SELECTED INTERFACE: {pcapDevice.Description}");
        }

        /// <summary>
        /// The function sends the given packet
        /// </summary>
        /// <param name="packetToSend">The packet to send</param>
        public static void SendPacket(Packet packetToSend)
        {
            using (PacketCommunicator communicator = pcapDevice.Open(100, // name of the device
                                 PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                 1000)) // read timeout
            {
                communicator.SendPacket(packetToSend);
            }
        }

        /// <summary>
        /// The fucntion handles the packets recieves by a handle to a function that it gets
        /// </summary>
        /// <param name="count"></param>
        /// <param name="callback">Handle to a function</param>
        public static void ReceivePackets(int count, HandlePacket callback)
        {
            using (PacketCommunicator communicator =
            pcapDevice.Open(65536,                         // portion of the packet to capture
                                                           // 65536 guarantees that the whole packet will be captured on all the link layers
                    PacketDeviceOpenAttributes.Promiscuous,  // promiscuous mode
                    1000))                                  // read timeout
            {
#if DEBUG
                Console.WriteLine("[LISTENING] " + pcapDevice.Description + "...");
#endif
                communicator.ReceivePackets(0, callback);
            }
        }

        /// <summary>
        /// The function builds the ethernet layer
        /// </summary>
        /// <param name="sourceMac">Source mac</param>
        /// <param name="dstMac">Destination mac</param>
        /// <returns>Ethernet layer object</returns>
        public static EthernetLayer BuildEthernetLayer(string sourceMac = "7C:B0:C2:FE:0F:C5", string dstMac = "7C:B0:C2:FE:0F:C5")
        {
            return
            new EthernetLayer
            {
                Source = new MacAddress(sourceMac),
                Destination = new MacAddress(dstMac),
                EtherType = EthernetType.None, // Will be filled automatically.
            };
        }

        /// <summary>
        /// The function builds the ip layer
        /// </summary>
        /// <param name="sourceIp">Source ip</param>
        /// <param name="dstIp">Destination ip</param>
        /// <returns>Ip layer object</returns>
        public static IpV4Layer BuildIpv4Layer(string sourceIp = "127.0.0.1", string dstIp = "127.0.0.1")
        {
            return
            new IpV4Layer
            {
                Source = new IpV4Address(sourceIp),
                CurrentDestination = new IpV4Address(dstIp),
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
        /// <param name="dstPort">Destination port</param>
        /// <returns>Transport layer object (udp)</returns>
        public static UdpLayer BuildUdpLayer(ushort sourcePort = SERVER_PORT, ushort dstPort = 10000)
        {
            return
            new UdpLayer
            {
                SourcePort = sourcePort,
                DestinationPort = dstPort,
                Checksum = null, // Will be filled automatically.
                CalculateChecksumValue = true,
            };
        }

        public static PayloadLayer BuildPLayer(string data = "")
        {
            return new PayloadLayer
            {
                Data = new Datagram(Encoding.ASCII.GetBytes(data))
            };
        }

        /// <summary>
        /// The function converts a byte list into a payload layer
        /// </summary>
        /// <param name="data">Data to convert</param>
        /// <returns>Payload layer object</returns>
        public static PayloadLayer BuildPLayer(List<byte[]> data)
        {
            #region https://stackoverflow.com/questions/4875968/concatenating-a-c-sharp-list-of-byte
            byte[] output = new byte[data.Sum(arr => arr.Length)];
            int writeIdx = 0;

            foreach (byte[] byteArr in data)
            {
                byteArr.CopyTo(output, writeIdx);
                writeIdx += byteArr.Length;
            }
            #endregion

            return new PayloadLayer
            {
                Data = new Datagram(output)
            };
        }

        /// <summary>
        /// The function converts a byte array into a payload layer
        /// </summary>
        /// <param name="data">Data to convert</param>
        /// <returns>Payload layer object</returns>
        public static PayloadLayer BuildPLayer(byte[] data)
        {
            return new PayloadLayer
            {
                Data = new Datagram(data)
            };
        }

        /// <summary>
        /// The function builds all of the base layers (ehternet, ip, transport)
        /// </summary>
        /// <param name="source_port">Source port</param>
        /// <param name="destination_port">Destination port</param>
        /// <returns>List of the base layers</returns>
        public static ILayer[] BuildBaseLayers(ushort source_port, ushort destination_port)
        {
            ILayer[] baseLayers = new ILayer[3];

            baseLayers[0] = BuildEthernetLayer();
            baseLayers[1] = BuildIpv4Layer();
            baseLayers[2] = BuildUdpLayer(source_port, destination_port);

            return baseLayers;
        }
    }
}