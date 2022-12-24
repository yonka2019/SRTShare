using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
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
        internal const uint SERVER_SOCKET_ID = 123;
        internal static Dictionary<uint, SRTSocket> SRTSockets = new Dictionary<uint, SRTSocket>();
        // SRTSockets: (example)
        // [0] : SRTSocket
        // [SOCKET_ID] : SRTSocket

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


        /// <summary>
        /// The function starts receiving the packets
        /// </summary>
        private static void RecvP()
        {
            PacketManager.ReceivePackets(0, HandlePacket);
        }


        /// <summary>
        /// Callback function invoked by Pcap.Net for every incoming packet
        /// </summary>
        /// <param name="packet">New given packet</param>
        private static void HandlePacket(Packet packet)
        {
            if (packet.IsValidUDP(PacketManager.SERVER_PORT))  // UDP Packet
            {
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Handshake handshake_request = new Handshake(payload);

                        if (handshake_request.TYPE == (uint)Handshake.HandshakeType.INDUCTION) // [client -> server] (SRT) Induction
                        {
                            RequestsHandler.HandleInduction(packet, handshake_request, datagram);
                        }

                        else if (handshake_request.TYPE == (uint)Handshake.HandshakeType.CONCLUSION) // [client -> server] (SRT) Conclusion
                        {
                            RequestsHandler.HandleConclusion(packet, handshake_request, datagram);
                            SRTSockets[handshake_request.SOCKET_ID].KeepAlive.StartCheck(); // start keep-alive checking
                            SRTSockets[handshake_request.SOCKET_ID].Data.StartVideo(); // start keep-alive checking

                            // START VIDEO HERE!!


                            // START KEEP-ALIVE EACH 3 SECONDS TO CLIENT TO REAFFRIM CONNECTION :

                            /* KEEP-ALIVE GOOD TRANSMISSION PREVIEW: 
                             * [SERVER] -> [CLIENT] (keep-alive check request)
                             * [CLIENT -> [SERVER] (keep-alive check confirm)
                             * --------------------
                             * [!] EACH 3 SECONDS [!]
                             */

                            /* KEEP-ALIVE BAD TRANSMISSION PREVIEW: 
                             * [SERVER] -> [CLIENT] (keep-alive check request)
                             * . . . (5 seconds passed, no check confirm)
                             * [SERVER] CLOSE [client] SOCKET, DISPOSE RESOURCES
                             */
                        }
                    }

                    else if (Shutdown.IsShutdown(payload))  // (SRT) Shutdown
                    {
                        RequestsHandler.HandleShutDown(packet);
                    }

                    else if (KeepAlive.IsKeepAlive(payload))  // (SRT) KeepAlive
                    {
                        uint clientSocketId = ProtocolManager.GenerateSocketId(packet.Ethernet.IpV4.Source.ToString(), packet.Ethernet.IpV4.Udp.SourcePort);

                        if (SRTSockets.ContainsKey(clientSocketId))
                            SRTSockets[clientSocketId].KeepAlive.ConfirmStatus();  // sign as alive
                    }
                }
            }

            else if (packet.IsValidARP())  // ARP Packet
            {
                if (packet.Ethernet.Arp.TargetProtocolIpV4Address.ToString() == PacketManager.SERVER_IP) // the arp was for the server
                {
                    ArpDatagram arp = packet.Ethernet.Arp;
                    Packet arpReply = ARPManager.Reply(PacketManager.device, BitConverter.ToString(arp.SenderHardwareAddress.ToArray()).Replace("-", ":"), arp.SenderProtocolIpV4Address.ToString());
                    PacketManager.SendPacket(arpReply);
                }
            }
        }


        internal static void LostConnection(uint socket_id)
        {
            Console.WriteLine($"[{socket_id}] is dead");
        }


//        private static void ShotBuildSend(PacketDevice device, ushort dstPort)
//        {
//            using (PacketCommunicator communicator = device.Open(100, // name of the device
//                                                         PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
//                                                         1000)) // read timeout
//            {
//                List<Packet> imageChunks = SplitToPackets(dstPort);
//                int chunk_counter = -1;
//                int total_chunks = imageChunks.Count - 1;

//                Console.WriteLine($"[SEND : {dstPort}] Image (Total chunks: {total_chunks})"); // each image
//                foreach (Packet chunk in imageChunks)
//                {
//                    communicator.SendPacket(chunk);
//#if DEBUG
//                    Console.WriteLine($"[SEND : {dstPort}] Chunk number: {++chunk_counter}/{total_chunks} | Size: {chunk.Count}"); // each chunk
//#endif
//                }
//                Console.WriteLine("--------------------\n\n\n");
//            }
//        }

        //private static List<Packet> SplitToPackets(ushort dstPort)
        //{
        //    Bitmap bmp = TakeScreenShot();
        //    MemoryStream mStream = GetJpegStream(bmp);

        //    List<byte> stream = mStream.ToArray().ToList();
        //    List<Packet> packets = new List<Packet>();
        //    List<byte> packet_id; // packet id have same meaning as 'chunk number'
        //    List<byte> total_chunks_number;
        //    List<byte> packet_data;
        //    int i;

        //    EthernetLayer ethernetLayer = PcapFunc.BuildEthernetLayer();
        //    IpV4Layer ipV4Layer = PcapFunc.BuildIpv4Layer();
        //    UdpLayer udpLayer = PcapFunc.BuildUdpLayer(PcapFunc.SERVER_PORT, dstPort);

        //    for (i = 1000; (i + 1000) < stream.Count; i += 1000) // 1000 bytes iterating
        //    {
        //        packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
        //        total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
        //        packet_data = stream.GetRange(i - 1000, 1000);

        //        packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
        //        packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][data] // FINAL

        //        PayloadLayer p1 = PcapFunc.BuildPLayer(packet_id.ToArray());
        //        packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p1).Build(DateTime.Now));
        //    }

        //    packet_id = BitConverter.GetBytes((ushort)((i - 1000) / 1000)).ToList();
        //    total_chunks_number = BitConverter.GetBytes((ushort)((stream.Count / 1000) - 1)).ToList();
        //    packet_data = stream.GetRange(i, stream.Count - i);

        //    packet_id.AddRange(total_chunks_number); // [packet id - (2bytes)][chunks number - (2bytes)]
        //    packet_id.AddRange(packet_data); // [packet id - (2bytes)][chunks number - (2bytes)][last data]

        //    PayloadLayer p2 = PcapFunc.BuildPLayer(packet_id.ToArray());
        //    packets.Add(new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, p2).Build(DateTime.Now));

        //    return packets;
        //}


        /// <summary>
        /// The function takes a screen shot 
        /// </summary>
        /// <returns>Bitmap obejct with the screenshot</returns>
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


        /// <summary>
        /// The function converts a bitmap obejct into a memory stream (easier to send)
        /// </summary>
        /// <param name="bmp">Bitmap object to convert</param>
        /// <returns>Memory stream of the screenShot</returns>
        public static MemoryStream GetJpegStream(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

            return stream;
        }


        /// <summary>
        /// The function creates an encoder to convert the Bitmap object
        /// </summary>
        /// <param name="format">A format to convert to (jpeg in this case)</param>
        /// <returns>ImageCodecInfo object</returns>
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