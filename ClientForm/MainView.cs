using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class MainView : Form
    {
        private static uint packet_length = 0;
        private static readonly List<byte> data = new List<byte>();
        private static PacketCommunicator communicator;

        public MainView()
        {
            InitializeComponent();
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
                Console.Write(i + 1 + ". " + device.Name);
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
            Console.WriteLine(data.Count);
            Console.ReadKey();
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == 6969)
            {
                MemoryStream stream = datagram.Payload.ToMemoryStream();
                byte[] byteStream = stream.ToArray();
                Console.WriteLine(byteStream.Length);

                if (packet_length == 0) // first packet ([length][data])
                {
                    packet_length = BitConverter.ToUInt32(byteStream, 0);
                    data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());
                }
                else if (byteStream.Length < 1000) // last data packet
                {
                    communicator.Break();
                    packet_length = 0; // reset

                    data.AddRange(byteStream);

                    ShowImage();
                }
                else // data packet
                    data.AddRange(byteStream);
            }
        }

        private void ShowImage()
        {
            using (MemoryStream ms = new MemoryStream(data.ToArray()))
            {
                pictureBox1.Image = new Bitmap(Image.FromStream(ms));
            }
        }
    }
}
