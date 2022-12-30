using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SRTLibrary
{
    public static class PacketManager
    {
        public static readonly LivePacketDevice Device;  // active network interface
        public static readonly string LocalIp;
        public static readonly string MacAddress;
        public static readonly string DefaultGateway;
        public static string Mask { get; private set; }

        static PacketManager()
        {
            LocalIp = GetActiveLocalIp();
            Device = AutoSelectNetworkInterface(LocalIp);
            MacAddress = Device.GetMacAddress().ToString().Replace("-", ":");
            DefaultGateway = Device.GetNetworkInterface().GetIPProperties().GatewayAddresses.Where(inter => inter.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().Address.ToString();
            Console.WriteLine($"####################\n[!] SELECTED INTERFACE: {Device.Description}\n" +
                $"* Local IP: {LocalIp}\n" +
                $"* MAC: {MacAddress}\n" +
                $"* Gateway: {DefaultGateway}\n" +
                $"* Mask: {Mask}\n####################\n\n");
        }

        /// <summary>
        /// The function gets the local ip of the computer
        /// </summary>
        /// <returns>The computer's local ip</returns>
        private static string GetActiveLocalIp()
        {
            IPAddress localAddress = null;

            try
            {
                UdpClient u = new UdpClient("8.8.8.8", 1);
                localAddress = ((IPEndPoint)u.Client.LocalEndPoint).Address;
            }
            catch
            {
                Console.WriteLine("[ERROR] Can't find local IP");  // there is no valid NI (Network Interface)
                Console.ReadKey();
                Environment.Exit(0);
            }

            return localAddress.ToString();
        }

        /// <summary>
        /// The function auto selects the device where all the messages will be sent to
        /// </summary>
        /// <param name="activeLocalIp">Local ip</param>
        /// <returns>Device where all the messages will be sent to</returns>
        private static LivePacketDevice AutoSelectNetworkInterface(string activeLocalIp)
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int selectDeviceIndex = -1;

            // iterate interfaces list and found the right one
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                foreach (DeviceAddress deviceAddress in device.Addresses)
                {
                    if (deviceAddress.Address.ToString().Contains(activeLocalIp))
                    {
                        Mask = deviceAddress.Netmask.ToString().Replace("Internet ", "");

                        selectDeviceIndex = i + 1;
                        break;
                    }
                }
            }

            if (allDevices.Count == 0)
            {
                Console.WriteLine("[ERROR] No interfaces found");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (selectDeviceIndex == -1)
            {
                Console.WriteLine($"[ERROR] There is no interface which matches with the local ip address");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Take the selected adapter
            return allDevices[selectDeviceIndex - 1];
        }

        /// <summary>
        /// The function sends the given packet
        /// </summary>
        /// <param name="packetToSend">The packet to send</param>
        public static void SendPacket(Packet packetToSend)
        {
            using (PacketCommunicator communicator = Device.Open(100, // name of the device
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
            Device.Open(65536,                         // portion of the packet to capture
                                                       // 65536 guarantees that the whole packet will be captured on all the link layers
                    PacketDeviceOpenAttributes.Promiscuous,  // promiscuous mode
                    1000))                                  // read timeout
            {
#if DEBUG
                Console.WriteLine("[LISTENING]");
#endif
                communicator.ReceivePackets(0, callback);
            }
        }

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