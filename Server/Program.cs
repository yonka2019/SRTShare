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
using System.Threading;

/*
 * PACKET STRUCTURE:
 * // [PACKET ID (CHUNK NUMBER)]  [TOTAL CHUNKS NUMBER]  [DATA / LAST DATA] //
 * //       [2 BYTES]                   [2 BYTES]          [>=1000 BYTES]   //
 */

namespace Server
{
    internal class Program
    {
        private static readonly Dictionary<int, Thread> connections = new Dictionary<int, Thread>(); // <DST.PORT : THREAD[Video()]
        private static PacketDevice selectedDevice;
        private static PacketCommunicator communicator;

        private static class Win32Native
        {
            public const int DESKTOPVERTRES = 0x75;
            public const int DESKTOPHORZRES = 0x76;

            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hDC, int index);
        }

        private static void Main()
        {
            selectedDevice = PcapFunc.pcapDevice;

            new Thread(new ThreadStart(RecvP)).Start(); // always listen for any new connections
        }

        private static void Video(object dstPort)
        {
            while (true)
            {
                ShotBuildSend(selectedDevice, (ushort)dstPort);
            }
        }

        private static void ShotBuildSend(PacketDevice device, ushort dstPort)
        {
            using (PacketCommunicator communicator = device.Open(100, // name of the device
                                                         PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                         1000)) // read timeout
            {
                List<Packet> imageChunks = SplitToPackets(dstPort);
                int chunk_counter = -1;
                int total_chunks = imageChunks.Count - 1;

                Console.WriteLine($"[SEND : {dstPort}] Image (Total chunks: {total_chunks})"); // each image
                foreach (Packet chunk in imageChunks)
                {
                    communicator.SendPacket(chunk);
#if DEBUG
                    Console.WriteLine($"[SEND : {dstPort}] Chunk number: {++chunk_counter}/{total_chunks} | Size: {chunk.Count}"); // each chunk
#endif
                }
                Console.WriteLine("--------------------\n\n\n");
            }
        }

        private static void RecvP()
        {
            // open the device
            using (communicator =
            selectedDevice.Open(65536,                         // portion of the packet to capture
                                                               // 65536 guarantees that the whole packet will be captured on all the link layers
                    PacketDeviceOpenAttributes.Promiscuous,  // promiscuous mode
                    1000))                                  // read timeout
            {
                Console.WriteLine("[LISTENING] " + selectedDevice.Description + "...");
                communicator.ReceivePackets(0, HandlePacket);
            }
        }

        private static void HandlePacket(Packet packet)
        { // check by data which packet is this (control/data)
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.DestinationPort == PcapFunc.SERVER_PORT)
            {
                // if () // check if packet is beginning handshake [NEW CONNECTION]
                if (!connections.ContainsKey(datagram.SourcePort))
                    connections.Add(datagram.SourcePort, new Thread(new ParameterizedThreadStart(Video)));

                if (connections.ContainsKey(datagram.SourcePort))
                    connections[datagram.SourcePort].Start(datagram.SourcePort); // start video
            }
        }

        private static List<Packet> SplitToPackets(ushort dstPort)
        {
            Bitmap bmp = TakeScreenShot();
            MemoryStream mStream = GetJpegStream(bmp);

            List<byte> stream = mStream.ToArray().ToList();
            List<Packet> packets = new List<Packet>();
            List<byte> packet_id; // packet id have same meaning as 'chunk number'
            List<byte> total_chunks_number;
            List<byte> packet_data;
            int i;

            EthernetLayer ethernetLayer = PcapFunc.BuildEthernetLayer();

            IpV4Layer ipV4Layer = PcapFunc.BuildIpv4Layer();

            UdpLayer udpLayer = PcapFunc.BuildUdpLayer(PcapFunc.SERVER_PORT, dstPort);

            for (i = 1000; (i + 1000) < stream.Count; i += 1000) // 1000 bytes iterating
            {
                packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
                total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
                packet_data = stream.GetRange(i - 1000, 1000);

                packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
                packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][data] // FINAL

                PayloadLayer p1 = new PayloadLayer // [data (1000 bytes each chunk)]
                {
                    Data = new Datagram(packet_id.ToArray())
                };
                packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p1).Build(DateTime.Now));
            }

            packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
            total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
            packet_data = stream.GetRange(i, stream.Count - i);

            packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
            packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][last data]

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