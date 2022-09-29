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

                int deviceIndex;
                do
                {
                    Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                    string deviceIndexString = Console.ReadLine();
                    if (deviceIndexString == "q") return;
                    if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                        deviceIndex < 1 || deviceIndex > allDevices.Count)
                    {
                        deviceIndex = 0;
                    }
                } while (deviceIndex == 0);

                // Take the selected adapter
                PacketDevice selectedDevice = allDevices[deviceIndex - 1];

                try
                {
                    using (PacketCommunicator communicator = selectedDevice.Open(100, // name of the device
                                                                             PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                                             1000)) // read timeout
                    {
                        List<Packet> a = SplitToPackets();
                        Console.WriteLine(a.Count);

                        foreach (Packet p in a)
                        {
                            communicator.SendPacket(p);
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
            MemoryStream stream = GetJpegStream(bmp);

            List<byte> str = stream.ToArray().ToList();
            int p_length = str.Count;
            Console.WriteLine("Total Length: " + p_length);

            List<Packet> packets = new List<Packet>();
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

            List<byte> length = BitConverter.GetBytes(p_length).ToList();
            List<byte> data = str.GetRange(0, 1000);
            length.AddRange(data); // [data length - (4bytes)][data]

            PayloadLayer p1 = new PayloadLayer // [data length(4bytes)][data(1000)]
            {

                Data = new Datagram(length.ToArray())
            };
            packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p1).Build(DateTime.Now));


            for (i = 2000; (i + 1000) < str.Count; i += 1000)
            {
                PayloadLayer p2 = new PayloadLayer // [data (1000)]
                {
                    Data = new Datagram(str.GetRange(i - 1000, 1000).ToArray())
                };
                packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p2).Build(DateTime.Now));
            }


            PayloadLayer p3 = new PayloadLayer // [last data]
            {
                Data = new Datagram(str.GetRange(i, str.Count - i).ToArray())
            };
            packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p3).Build(DateTime.Now));

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
