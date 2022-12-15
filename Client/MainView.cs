using PcapDotNet.Base;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Transport;
using SRTManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using SRTControl = SRTManager.ProtocolFields.Control;
using SRTRequest = SRTManager.RequestsFactory;

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
        private static string server_mac;
        private static uint client_socket_id = 0;



        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 50000);

            //  start receiving packets
            pRecvThread = new Thread(new ThreadStart(RecvP));
            pRecvThread.Start();

            Packet arpRequest = ARPManager.Request(PacketManager.device, Encoding.ASCII.GetBytes(PacketManager.SERVER_IP.IPAddress).AsReadOnly());
            PacketManager.SendPacket(arpRequest);

            SRTRequest.HandshakeRequest handshake = new SRTRequest.HandshakeRequest
                (PacketManager.BuildBaseLayers(myPort, PacketManager.SERVER_PORT));

            DateTime now = DateTime.Now;

            client_socket_id = ProtocolManager.GenerateSocketId(PacketManager.LOOPBACK_IP.IPAddress, myPort);
            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.LOOPBACK_IP.IPAddress, myPort, now), init_psn: 0, p_ip: PacketManager.LOOPBACK_IP.IPAddress.GetUInt32(), clientSide: true, client_socket_id, 0); // *** need to change peer id***

            /*Packet packet = new PacketBuilder(PacketManager.BuildEthernetLayer(),
                PacketManager.BuildIpv4Layer(),
                PacketManager.BuildUdpLayer(myPort, PacketManager.SERVER_PORT),
                PacketManager.BuildPLayer("Start transmission")).Build(DateTime.Now);
            */

            PacketManager.SendPacket(handshake_packet);
        }

        /// <summary>
        /// The function starts receiving the packets
        /// </summary>
        private void RecvP()
        {
            PacketManager.ReceivePackets(0, PacketHandler);
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every incoming packet
        /// </summary>
        /// <param name="packet">New given packet</param>
        private void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == PacketManager.SERVER_PORT && datagram.DestinationPort == myPort)
            {
                byte[] payload = datagram.Payload.ToArray();

                if (SRTControl.SRTHeader.IsControl(payload)) // check if control
                {
                    if (SRTControl.Handshake.IsHandshake(payload)) // check if handshake
                    {
                        SRTControl.Handshake handshake_request = new SRTControl.Handshake(payload);

                        if (handshake_request.TYPE == (uint)SRTControl.Handshake.HandshakeType.INDUCTION) // server -> client (induction)
                        {
                            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(PacketManager.LOOPBACK_IP.IPAddress, myPort, DateTime.Now))
                            {
                                SRTRequest.HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest(PacketManager.BuildBaseLayers(myPort, PacketManager.SERVER_PORT));

                                // client -> server (conclusion)
                                Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: PacketManager.LOOPBACK_IP.IPAddress.GetUInt32(), clientSide: true, client_socket_id, handshake_request.SOCKET_ID, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                                PacketManager.SendPacket(handshake_packet);
                            }

                            else
                            {
                                // Exit the prgram and send a shutdwon request
                                SRTRequest.ShutDownRequest shutdown_response = new SRTRequest.ShutDownRequest(PacketManager.BuildBaseLayers(myPort, PacketManager.SERVER_PORT));
                                Packet shutdown_packet = shutdown_response.Exit();
                                PacketManager.SendPacket(shutdown_packet);

                                MessageBox.Show("Wrong cookie - Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                        }
                    }
                }

                else
                {
                    ArpDatagram arp = packet.Ethernet.Arp;
                    server_mac = arp.SenderHardwareAddress.ToString();
                    Console.WriteLine("!!!" + server_mac);

                    //MemoryStream stream = datagram.Payload.ToMemoryStream();
                    //byte[] byteStream = stream.ToArray();

                    //// [0][1]
                    //current_packet_id = BitConverter.ToUInt16(byteStream, 0); // take first two bytes of the chunk | --> ([ID (2 bytes)] <-- [TOTAL CHUNKS NUMBER (2 bytes)][DATA] |

                    //// [2][3]
                    //total_chunks_number = BitConverter.ToUInt16(byteStream, 2); // take second two bytes of the chunk | ([ID (2 bytes)] --> [TOTAL CHUNKS NUMBER (2 bytes)] <-- [DATA] |

                    ////Console.WriteLine($"[GOT] Chunk number: {current_packet_id}/{total_chunks_number} | Size: {byteStream.Length}"); // each chunk print

                    //if (current_packet_id == total_chunks_number) // last chunk of image received
                    //{
                    //    imageBuilt = true;
                    //    data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());
                    //    ShowImage(true);
                    //    data.Clear(); // prepare to next chunk
                    //}
                    //// new image chunks started (IDs: [OLD IMAGE] 1 2 3 4 [NEW IMAGE (~show old image~)] 1 2 . . .
                    //else if (current_packet_id < last_packet_id && !imageBuilt) // if the above condition didn't done (packet loss) and the image was changed, show the image
                    //{
                    //    if (!firstChunk)
                    //        ShowImage(false); // show image if all his chunks arrived
                    //    else
                    //        firstChunk = false;

                    //    data.Clear(); // clear all data from past images
                    //    data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());
                    //}
                    //else // next packets (same chunk continues)
                    //    data.AddRange(byteStream.Skip(4).Take(byteStream.Length - 4).ToList());

                    //last_packet_id = current_packet_id;
                }

            }
        }

        /// <summary>
        /// The function accurs when the form is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
            SRTRequest.ShutDownRequest shutdown_response = new SRTRequest.ShutDownRequest(PacketManager.BuildBaseLayers(myPort, PacketManager.SERVER_PORT));
            Packet shutdown_packet = shutdown_response.Exit();
            PacketManager.SendPacket(shutdown_packet);

            MessageBox.Show("Sent a ShutDown request!",
                "Bye Bye", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            base.OnClosed(e);
        }


        /// <summary>
        /// The function shows the server's screen if all the chunks arrived
        /// </summary>
        /// <param name="allChunksReceived">True if all chunks were received, false if not</param>
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
