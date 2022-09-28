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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace ConsoleApp4
{
    internal class Program
    {
        static class Win32Native
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

                int deviceIndex = 0;
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
                // Open the output device

                catch (Exception ex) { Console.WriteLine("Wrong device\n--------------------\n\n" + ex + "\n\n\n"); }

            }

        }

        private static List<Packet> SplitToPackets()
        {
            Bitmap bmp = TakeScreenShot();
            MemoryStream stream = GetJpegStream(bmp);

            List<byte> str = stream.ToArray().ToList();
            int p_length = str.Count;
            Console.WriteLine(p_length);


           
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
            var l = BitConverter.GetBytes(p_length).ToList();
            var d = str.GetRange(0, 1000);
            l.AddRange(d);


            PayloadLayer payloadLayer =
                new PayloadLayer
                {

                    Data = new Datagram(l.ToArray())
                };
            List<Packet> packets = new List<Packet>();

            packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer).Build(DateTime.Now));
            int i;
            for (i = 2000; (i + 1000) < str.Count; i += 1000)
            {
                PayloadLayer p = new PayloadLayer
                {
                    Data = new Datagram(str.GetRange(i - 1000, 1000).ToArray())
                };

                packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p).Build(DateTime.Now));
            }

            PayloadLayer p2 = new PayloadLayer
            {
                Data = new Datagram(str.GetRange(i, str.Count - i).ToArray())
            };

            packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p2).Build(DateTime.Now));


            return packets;
        }

        static Bitmap TakeScreenShot()
        {
            int width, height;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var hDC = g.GetHdc();
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

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

            //bmp.Save(stream, ImageFormat.Jpeg);

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
