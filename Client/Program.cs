using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System.Net.NetworkInformation;
using System.IO;

namespace ConsoleApp5
{
    class Program
    {
        private static uint p_length = 0;
        private static List<byte> data = new List<byte>();
        private static PacketCommunicator communicator;

        static void Main(string[] args)
        {
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                Console.Write((i + 1) + ". " + device.Name);
                if (device.Description != null)
                    Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }

            int deviceIndex = 0;
            do
            {
                Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                string deviceIndexString = Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

            // Open the device
            using (communicator =
                selectedDevice.Open(65536,                                  // portion of the packet to capture
                                                                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                Console.WriteLine("Listening on " + selectedDevice.Description + "...");

                // start the capture
                communicator.ReceivePackets(0, PacketHandler);
            }
            Console.WriteLine("a: " + data.Count);
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private static void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            
            if (datagram != null && datagram.SourcePort == 6969)
            {
                Console.WriteLine(data.Count);
                var a = datagram.Payload.ToMemoryStream();
                var b = a.ToArray();

                if (p_length == 0)
                {
                    data = new List<byte>();
                    p_length = BitConverter.ToUInt32(b, 0);
                    data.AddRange(b.Skip(4).Take(b.Length - 4).ToList());
                }
                else if (p_length == data.Count)
                {
                    p_length = 0; // reset
                    communicator.Break();
                    Console.WriteLine(123123);
                }
                else
                {
                    //Console.WriteLine(data.Count);
                    data.AddRange(b);

                }
            }
        }
    }
}
