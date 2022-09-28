using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ClientForm
{
    public partial class Form1 : Form
    {
        private static uint p_length = 0;
        private static List<byte> data = new List<byte>();
        private static PacketCommunicator communicator;
        static int c = 0;
        Task screen = null;

        public Form1()
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
            Console.WriteLine(data.Count);
            Console.ReadKey();
        }
        // Callback function invoked by Pcap.Net for every incoming packet
        private  void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == 6969)
            {
                //Console.WriteLine(data.Count);
                var a = datagram.Payload.ToMemoryStream();
                var b = a.ToArray();
                Console.WriteLine(b.Length);

                if (p_length == 0)
                {
                    p_length = BitConverter.ToUInt32(b, 0);
                    data.AddRange(b.Skip(4).Take(b.Length - 4).ToList());
                }
                else if (b.Length < 1000) // last packet received
                {
                    data.AddRange(b);
                    p_length = 0; // reset
                    using (var ms = new MemoryStream(data.ToArray()))
                    {
                        pictureBox1.Image = new Bitmap(Image.FromStream(ms));
                   
                    }
                    communicator.Break();

                }
                else
                {
                    //Console.WriteLine(data.Count);
                    data.AddRange(b);
                    //Console.WriteLine(++c);

                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e) //Connect
        {
           

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        public Bitmap ConvertToBitmap(Stream stream)
        {
            Bitmap bitmap;

            Image image = Image.FromStream(stream);
            bitmap = new Bitmap(image);

            return bitmap;
        }
    }
}
