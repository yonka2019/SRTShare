using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using C_STRHeader = SRTManager.ProtocolFields.Control.SRTHeader;
using Handshake = SRTManager.ProtocolFields.Control.Handshake;

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

        private static class Win32Native
        {
            public const int DESKTOPVERTRES = 0x75;
            public const int DESKTOPHORZRES = 0x76;

            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hDC, int index);
        }

        private static void Main()
        {
            new Thread(new ThreadStart(RecvP)).Start(); // always listen for any new connections
        }

        private static void Video(object dstPort)
        {
            while (true)
            {
                ShotBuildSend(PacketManager.pcapDevice, (ushort)dstPort);
            }
        }

        private static void ShotBuildSend(PacketDevice device, ushort dstPort)
        {
            List<Packet> imageChunks = SplitToPackets(dstPort);
            int total_chunks = imageChunks.Count - 1;

            Console.WriteLine($"[SEND : {dstPort}] Image (Total chunks: {total_chunks})"); // each image
            foreach (Packet chunk in imageChunks)
            {
                PacketManager.SendPacket(chunk);
            }
            Console.WriteLine("--------------------\n\n\n");
        }

        private static void RecvP()
        {
            PacketManager.ReceivePackets(0, HandlePacket);
        }

        private static void HandlePacket(Packet packet)
        { // check by data which packet is this (control/data): 'The type initializer for 'SRTManager.PacketManager' threw
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.DestinationPort == PacketManager.SERVER_PORT)
            {
                byte[] payload = datagram.Payload.ToArray();

                if (C_STRHeader.IsControl(payload)) // check if control
                {
                    if (Handshake.IsHandshake(payload)) // check if handshake
                    {
                        Handshake handshake_request = new Handshake(payload);


                        if (handshake_request.TYPE == (uint)Handshake.HandshakeType.INDUCTION) // client -> server (induction)
                        {
                            ProtocolManager.HandshakeRequest handshake_response = PacketManager.buildBasePacket(PacketManager.SERVER_PORT, datagram.SourcePort);

                            uint cookie = ProtocolManager.GenerateCookie("127.0.0.1", datagram.SourcePort, DateTime.Now); // need to save cookie somewhere

                            Packet handshake_packet = handshake_response.Induction(cookie, init_psn: 0, p_ip: 0, clientSide: false); // ***need to change peer id***
                            PacketManager.SendPacket(handshake_packet);
                        }


                        else if (handshake_request.TYPE == (uint)Handshake.HandshakeType.CONCLUSION) // client -> server (conclusion)
                        {
                            ProtocolManager.HandshakeRequest handshake_response = PacketManager.buildBasePacket(PacketManager.SERVER_PORT, datagram.SourcePort);

                            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: 0, clientSide: false); // ***need to change peer id***
                            PacketManager.SendPacket(handshake_packet);
                        }
                    }
                }
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

            EthernetLayer ethernetLayer = PacketManager.BuildEthernetLayer();
            IpV4Layer ipV4Layer = PacketManager.BuildIpv4Layer();
            UdpLayer udpLayer = PacketManager.BuildUdpLayer(PacketManager.SERVER_PORT, dstPort);

            for (i = 1000; (i + 1000) < stream.Count; i += 1000) // 1000 bytes iterating
            {
                packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
                total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
                packet_data = stream.GetRange(i - 1000, 1000);

                packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
                packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][data] // FINAL

                PayloadLayer p1 = PacketManager.BuildPLayer(packet_id.ToArray());
                packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p1).Build(DateTime.Now));
            }

            packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
            total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
            packet_data = stream.GetRange(i, stream.Count - i);

            packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
            packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][last data]

            PayloadLayer p2 = PacketManager.BuildPLayer(packet_id.ToArray());
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