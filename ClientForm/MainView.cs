using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class MainView : Form
    {
        private static ushort current_packet_id = 0;
        private static ushort last_packet_id = 1;
        private static readonly bool firstImage = true;

        private static readonly List<byte> data = new List<byte>();
        private static PacketCommunicator communicator;

        private const string DEFAULT_INTERFACE_SUBSTRING = "Intel"; // default interface must contain this substring to be automatically chosen

        public MainView()
        {
            InitializeComponent();
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int deviceIndex = -1;

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
                    if (device.Description.Contains(DEFAULT_INTERFACE_SUBSTRING))
                    {
                        deviceIndex = i + 1;
                        Console.WriteLine("\n\n[!] Interface selected automatically: " + allDevices[deviceIndex - 1].Description);
                        Console.WriteLine("Press any button to continue..");
                        Console.ReadKey();
                        break;
                    }
                    else
                        Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }
            if (deviceIndex == -1)
            {
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
            }

            Thread pThread = new Thread(new ThreadStart(recvP));

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
                pThread.Start();

            }
            Console.ReadKey();
        }

        private void recvP()
        {
            communicator.ReceivePackets(0, PacketHandler);
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == 6969)
            {
                MemoryStream stream = datagram.Payload.ToMemoryStream();
                byte[] byteStream = stream.ToArray();

                current_packet_id = BitConverter.ToUInt16(byteStream, 0);
                Console.WriteLine("Got chunk number: " + current_packet_id);
                if (current_packet_id < last_packet_id) // new image (first chunk of the image)
                {
                    if (!firstImage)
                        ShowImage(); // show image if all his chunks arrived

                    data.Clear(); // clear all data from past images
                    data.AddRange(byteStream.Skip(2).Take(byteStream.Length - 2).ToList());
                }
                else // next packets (same chunk continues)
                    data.AddRange(byteStream.Skip(2).Take(byteStream.Length - 2).ToList());

                last_packet_id = current_packet_id;
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
