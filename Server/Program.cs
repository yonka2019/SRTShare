using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConsoleApp4
{
    internal class Program
    {
        private const string DEFAULT_INTERFACE_SUBSTRING = "Intel"; // default interface must contain this substring to be automatically chosen

        private static class Win32Native
        {
            public const int DESKTOPVERTRES = 0x75;
            public const int DESKTOPHORZRES = 0x76;

            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hDC, int index);
        }

        private static void Main(string[] args)
        {
            while (true)
            {
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
                            Console.WriteLine(); // blank line after readkey
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

                // Take the selected adapter
                PacketDevice selectedDevice = allDevices[deviceIndex - 1];

                try
                {
                    using (PacketCommunicator communicator = selectedDevice.Open(100, // name of the device
                                                                             PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                                             1000)) // read timeout
                    {
                        List<Packet> a = SplitToPackets();
                        int chunk_counter = 0;

                        foreach (Packet p in a)
                        {
                            communicator.SendPacket(p);
                            Console.WriteLine($"[SEND] Chunk number: {++chunk_counter} | Size: {p.Count}");

                        }
                        Console.WriteLine("--------------------\n\n\n");
                    }
                }
                catch (Exception ex) { Console.WriteLine("Wrong device\n--------------------\n\n" + ex + "\n\n\n"); }
            }
        }

        private static List<Packet> SplitToPackets()
        {
            Bitmap bmp = TakeScreenShot();
            MemoryStream mStream = GetJpegStream(bmp);

            List<byte> stream = mStream.ToArray().ToList();
            List<Packet> packets = new List<Packet>();
            List<byte> packet_id;
            List<byte> packet_data;
            int i;

            EthernetLayer ethernetLayer =
                new EthernetLayer
                {
                    Source = new MacAddress("7C:B0:C2:FE:0F:C5"),
                    Destination = new MacAddress("7C:B0:C2:FE:0F:C5"),
                    EtherType = EthernetType.None, // Will be filled automatically.
                };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = new IpV4Address("127.0.0.1"),
                    CurrentDestination = new IpV4Address("127.0.0.1"),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = 123,
                    Options = IpV4Options.None,
                    Protocol = null, // Will be filled automatically.
                    Ttl = 100,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 6969,
                    DestinationPort = 10000,
                    Checksum = null, // Will be filled automatically.
                    CalculateChecksumValue = true,
                };
            Console.WriteLine("TOTAL CHUNKS: " + (stream.Count / 1000));
            for (i = 1000; (i + 1000) < stream.Count; i += 1000)
            {
                packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
                packet_data = stream.GetRange(i - 1000, 1000);
                packet_id.AddRange(packet_data); // [packet id - (2bytes)][data]

                PayloadLayer p1 = new PayloadLayer // [data (1000 bytes each chunk)]
                {
                    Data = new Datagram(packet_id.ToArray())
                };
                packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p1).Build(DateTime.Now));
            }

            packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
            packet_data = stream.GetRange(i, stream.Count - i);
            packet_id.AddRange(packet_data); // [packet id - (2bytes)][last data]

            PayloadLayer p2 = new PayloadLayer // [last data]
            {
                Data = new Datagram(packet_id.ToArray())
            };
            packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p2).Build(DateTime.Now));

            return packets;
        }

        private static Bitmap TakeScreenShot()
        {
            int width, height;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hDC = g.GetHdc();
                width = Win32Native.GetDeviceCaps(hDC, Win32Native.DESKTOPHORZRES);
                height = Win32Native.GetDeviceCaps(hDC, Win32Native.DESKTOPVERTRES);
                g.ReleaseHdc(hDC);
            }

            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                return bmp;
            }
        }

        public static MemoryStream GetJpegStream(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();

            Encoder myEncoder = Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

            return stream;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}