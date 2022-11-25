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
        private const string DEFAULT_INTERFACE_SUBSTRING = "Oracle";  // default interface must contain this substring to be automatically chosen

        static PacketManager()
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int deviceIndex = -1;

            if (allDevices.Count == 0)
                return;

            // iterate interfaces list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                if (device.Description != null)
                {
                    if (device.Description.Contains(DEFAULT_INTERFACE_SUBSTRING))  // select the interface according the substring
                    {
                        deviceIndex = i + 1;
                        break;
                    }
                }
            }

            // Take the selected adapter
            pcapDevice = allDevices[deviceIndex - 1];
            Console.WriteLine($"[!] SELECTED INTERFACE: {pcapDevice.Description}");
        }
        public static void SendPacket(Packet packetToSend)
        {
            using (PacketCommunicator communicator = pcapDevice.Open(100, // name of the device
                                 PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                 1000)) // read timeout
            {
                communicator.SendPacket(packetToSend);
            }
        }
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

        public static PayloadLayer BuildPLayer(byte[] data)
        {
            return new PayloadLayer
            {
                Data = new Datagram(data)
            };
        }
    }
}