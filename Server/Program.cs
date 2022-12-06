using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Ip;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using SRTManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

using SRTControl = SRTManager.ProtocolFields.Control;
using SRTRequest = SRTManager.RequestsFactory;

/*
 * PACKET STRUCTURE:
 * // [PACKET ID (CHUNK NUMBER)]  [TOTAL CHUNKS NUMBER]  [DATA / LAST DATA] //
 * //       [2 BYTES]                   [2 BYTES]          [>=1000 BYTES]   //
 */

namespace Server
{
    internal class Program
    {
        private static Dictionary<uint, SRTSocket> SRTSockets = new Dictionary<uint, SRTSocket>();
        // SRTSockets: (example)
        // [0] : IPAddress
        // [SOCKET_ID] : IPAddress

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

                if (SRTControl.SRTHeader.IsControl(payload)) // check if control
                {
                    if(SRTControl.Shutdown.IsShutdown(payload))
                    {
                        uint req_socket_id = SRTSockets.FirstOrDefault(x => x.Value.IPEP == 
                        new IPEndPoint(new IPAddress(packet.Ethernet.IpV4.Source.ToValue()), datagram.SourcePort)).Key;

                        Console.WriteLine(req_socket_id);
                    }

                    if (SRTControl.Handshake.IsHandshake(payload)) // check if handshake
                    {
                        SRTControl.Handshake handshake_request = new SRTControl.Handshake(payload);


                        if (handshake_request.TYPE == (uint)SRTControl.Handshake.HandshakeType.INDUCTION) // client -> server (induction)
                        {
                            SRTRequest.HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.SERVER_PORT, datagram.SourcePort));

                            uint cookie = ProtocolManager.GenerateCookie("127.0.0.1", datagram.SourcePort, DateTime.Now); // need to save cookie somewhere

                            Packet handshake_packet = handshake_response.Induction(cookie, init_psn: 0, p_ip: 0, clientSide: false); // ***need to change peer id***
                            PacketManager.SendPacket(handshake_packet);
                        }


                        else if (handshake_request.TYPE == (uint)SRTControl.Handshake.HandshakeType.CONCLUSION) // client -> server (conclusion)
                        {
                            SRTRequest.HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest
                                (PacketManager.BuildBaseLayers(PacketManager.SERVER_PORT, datagram.SourcePort));

                            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: 0, clientSide: false); // ***need to change peer id***
                            PacketManager.SendPacket(handshake_packet);

                            // ADD NEW SOCKET TO LIST 
                            uint new_socket_id = (uint)(SRTSockets.Count + 1);
                            SRTSockets.Add(new_socket_id, new SRTSocket(new IPEndPoint(new IPAddress(handshake_request.PEER_IP), datagram.SourcePort), 
                                new KeepAliveManager(new_socket_id, datagram.SourcePort)));
                            // SRTSockets: (example)
                            // [0] : ip1
                            // [1]: ip2
                            // ADDED:
                            // [2]: ip3


                            // START VIDEO HERE!!


                            // START KEEP-ALIVE EACH 1 SECOND TO CLIENT TO REAFFRIM CONNECTION :

                            SRTSockets[(uint)SRTSockets.Count].KeepAlive.StartCheck();

                            /* KEEP-ALIVE GOOD TRANSMISSION PREVIEW: 
                             * [SERVER] -> [CLIENT] (keep-alive check request)
                             * [CLIENT -> [SERVER] (keep-alive check confirm)
                             * --------------------
                             * [!] EACH SECOND [!]
                             */

                            /* KEEP-ALIVE BAD TRANSMISSION PREVIEW: 
                             * [SERVER] -> [CLIENT] (keep-alive check request)
                             * . . . (5 seconds passed, no check confirm)
                             * [SERVER] CLOSE [client] SOCKET, DISPOSE RESOURCES
                             */
                            

                        }
                    }
                }
            }
        }



        private static void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;

            while (SRTSockets.ContainsKey(u_dest_socket_id))  // if socket still exist, continue check keep-alive
            {
                SRTRequest.KeepAliveRequest keepAlive_request = new SRTRequest.KeepAliveRequest
                                (PacketManager.BuildBaseLayers(PacketManager.SERVER_PORT, (ushort)SRTSockets[u_dest_socket_id].IPEP.Port));

                Packet keepAlive_packet = keepAlive_request.Check(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);
                // need tobe continued ; Server/KeepAliveManager [count sent/confirmed, if not equal more than 5 sec - break connection]
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