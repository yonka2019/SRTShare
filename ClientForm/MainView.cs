﻿using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using SRTManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using F_Handshake = SRTManager.ProtocolFields.Control.Handshake;

/*
 * PACKET STRUCTURE:
 * // [PACKET ID (CHUNK NUMBER)]  [TOTAL CHUNKS NUMBER]  [DATA / LAST DATA] //
 * //       [2 BYTES]                   [2 BYTES]          [>=1000 BYTES]   //
 */

namespace ClientForm
{
    public partial class MainView : Form
    {
        private static ushort current_packet_id = 0; // packet id have same meaning as 'chunk number'
        private static ushort last_packet_id = 1; // packet id have same meaning as 'chunk number'
        private static ushort total_chunks_number = 0;

        private static bool firstChunk = true;
        private static bool imageBuilt;

        private static readonly List<byte> data = new List<byte>();
        private readonly Thread pRecvThread;
        private readonly Random rnd = new Random();

        private static ushort myPort = 0;

        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 5000);

            ProtocolManager.HandshakeRequest handshake = new ProtocolManager.HandshakeRequest(PacketManager.BuildEthernetLayer(), 
                PacketManager.BuildIpv4Layer(),
                PacketManager.BuildUdpLayer(myPort, PacketManager.SERVER_PORT));

            DateTime now = DateTime.Now;

            Packet handshake_packet = handshake.Induction(SRTManager.ProtocolManager.GenerateCookie("127.0.0.1", myPort, now), 0, 0, true, 0); // *** need to change peer id***

            /*Packet packet = new PacketBuilder(PacketManager.BuildEthernetLayer(),
                PacketManager.BuildIpv4Layer(),
                PacketManager.BuildUdpLayer(myPort, PacketManager.SERVER_PORT),
                PacketManager.BuildPLayer("Start transmission")).Build(DateTime.Now);
            */

            PacketManager.SendPacket(handshake_packet);

            pRecvThread = new Thread(new ThreadStart(RecvP));

            // start the capture
            pRecvThread.Start();
        }

        private void RecvP()
        {
            PacketManager.ReceivePackets(0, PacketHandler);
        }
        
        // Callback function invoked by Pcap.Net for every incoming packet
        private void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == PacketManager.SERVER_PORT && datagram.DestinationPort == myPort)
            {
                bool is_handshake = (datagram.Payload.Length == 38); // change to -> checking by control header
            
                if (is_handshake)
                {
                    F_Handshake handshake_request = new F_Handshake(datagram.Payload.ToArray());

                    if (handshake_request.TYPE == (uint)(F_Handshake.HandshakeType.INDUCTION)) // server -> client (induction)
                    {
                        if (handshake_request.SYN_COOKIE == SRTManager.ProtocolManager.GenerateCookie("127.0.0.1", myPort, DateTime.Now))
                        {
                            ProtocolManager.HandshakeRequest handshake_response = new ProtocolManager.HandshakeRequest(PacketManager.BuildEthernetLayer(),
                                PacketManager.BuildIpv4Layer(),
                                PacketManager.BuildUdpLayer(myPort, PacketManager.SERVER_PORT));

                            // client -> server (conclusion)
                            Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: 0, clientSide: true, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                            PacketManager.SendPacket(handshake_packet);
                        }

                        else
                        {
                            MessageBox.Show("Problem with cookie transmission...");
                        }

                    }
                }

                else
                {
                    MemoryStream stream = datagram.Payload.ToMemoryStream();
                    byte[] byteStream = stream.ToArray();

                    // [0][1]
                    current_packet_id = BitConverter.ToUInt16(byteStream, 0); // take first two bytes of the chunk | --> ([ID (2 bytes)] <-- [TOTAL CHUNKS NUMBER (2 bytes)][DATA] |

                    // [2][3]
                    total_chunks_number = BitConverter.ToUInt16(byteStream, 2); // take second two bytes of the chunk | ([ID (2 bytes)] --> [TOTAL CHUNKS NUMBER (2 bytes)] <-- [DATA] |

                    //Console.WriteLine($"[GOT] Chunk number: {current_packet_id}/{total_chunks_number} | Size: {byteStream.Length}"); // each chunk print

                    if (current_packet_id == total_chunks_number) // last chunk of image received
                    {
                        imageBuilt = true;
                        data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());
                        ShowImage(true);
                        data.Clear(); // prepare to next chunk
                    }
                    // new image chunks started (IDs: [OLD IMAGE] 1 2 3 4 [NEW IMAGE (~show old image~)] 1 2 . . .
                    else if (current_packet_id < last_packet_id && !imageBuilt) // if the above condition didn't done (packet loss) and the image was changed, show the image
                    {
                        if (!firstChunk)
                            ShowImage(false); // show image if all his chunks arrived
                        else
                            firstChunk = false;

                        data.Clear(); // clear all data from past images
                        data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());
                    }
                    else // next packets (same chunk continues)
                        data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());

                    last_packet_id = current_packet_id;
                }
                
            }
        }

        private void ShowImage(bool allChunksReceived)
        {
            Console.WriteLine($"[GOT : {myPort}] Image (Total chunks: {total_chunks_number})"); // each image

            if (allChunksReceived)
                Console.WriteLine("[IMAGE BUILT SUCCESSFULLY] SHOWING IMAGE\n--------------------\n\n\n");
            else
                Console.WriteLine("[CHUNKS MISSING] SHOWING IMAGE\n--------------------\n\n\n");

            using (MemoryStream ms = new MemoryStream(data.ToArray()))
            {
                pictureBox1.Image = new Bitmap(Image.FromStream(ms));
            }
        }
    }
}
