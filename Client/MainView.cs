using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.IpV4;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using SRTLibrary.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SRTControl = SRTLibrary.SRTManager.ProtocolFields.Control;
using SRTRequest = SRTLibrary.SRTManager.RequestsFactory;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using System.Runtime.InteropServices;

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

       private bool first = true;

        public MainView()
        {
            InitializeComponent();

            myPort = (ushort)rnd.Next(1, 50000);

            //  start receiving packets
            pRecvThread = new Thread(new ThreadStart(RecvP));
            pRecvThread.Start();

            Packet arpRequest = ARPManager.Request(PacketManager.device, PacketManager.SERVER_IP);
            PacketManager.SendPacket(arpRequest);
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

                if (SRTHeader.IsControl(payload)) // check if control
                {
                    if (Handshake.IsHandshake(payload)) // check if handshake
                    {
                        Handshake handshake_request = new SRTControl.Handshake(payload);

                        if (handshake_request.TYPE == (uint)Handshake.HandshakeType.INDUCTION) // server -> client (induction)
                        {
                            HandleInduction(handshake_request);
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


            else if (IsArp(packet))
            {
                ArpDatagram arp = packet.Ethernet.Arp;

                if (IsMyMac(arp) && first) // my mac, and this is the first time answering 
                {
                    if (arp.SenderProtocolIpV4Address.ToString() == PacketManager.SERVER_IP) // mac from server
                    {
                        server_mac = BitConverter.ToString(arp.SenderHardwareAddress.ToArray()).Replace("-", ":");

                        HandshakeRequest handshake = new SRTRequest.HandshakeRequest
                    (PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));

                        //create induction packet
                        DateTime now = DateTime.Now;

                        client_socket_id = ProtocolManager.GenerateSocketId(PacketManager.localIp, myPort);

                        IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
                        Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.localIp, myPort, now), init_psn: 0, p_ip: peer_ip, clientSide: true, client_socket_id, 0);

                        PacketManager.SendPacket(handshake_packet);

                        first = false;
                    }
                }
            }

        }

        /// <summary>
        /// The function checks if it's a valid arp packet
        /// </summary>
        /// <param name="packet">Packet to check</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsArp(Packet packet)
        {
            return packet.Ethernet.Arp != null && packet.Ethernet.Arp.IsValid && packet.Ethernet.Arp.TargetProtocolIpV4Address != null;
        }

        /// <summary>
        /// The function checks if the targeted mac is my mac
        /// </summary>
        /// <param name="arp">ArpDatagram object</param>
        /// <returns>True if it's my mac, false if not</returns>
        private bool IsMyMac(ArpDatagram arp)
        {
            return BitConverter.ToString(arp.TargetHardwareAddress.ToArray()).Replace("-", ":") == ARPManager.GetMyMac(PacketManager.device);
        }

        /// <summary>
        /// The function handles the induction phaze
        /// </summary>
        /// <param name="handshake_request">Handshake object</param>
        private void HandleInduction(Handshake handshake_request)
        {
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(PacketManager.localIp, myPort, DateTime.Now))
            {
                HandshakeRequest handshake_response = new SRTRequest.HandshakeRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));

                // client -> server (conclusion)
                IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
                Console.WriteLine("My ip string: " + PacketManager.localIp);
                Console.WriteLine("My ip address: " + peer_ip.ToString());

                Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: true, client_socket_id, handshake_request.SOCKET_ID, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                PacketManager.SendPacket(handshake_packet);
            }

            else
            {
                // Exit the prgram and send a shutdwon request
                ShutDownRequest shutdown_response = new SRTRequest.ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));
                Packet shutdown_packet = shutdown_response.Exit();
                PacketManager.SendPacket(shutdown_packet);

                MessageBox.Show("Wrong cookie - Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// The function accurs when the form is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // when the form is closed, it means the client left the conversation -> Need to send a shutdown request
            ShutDownRequest shutdown_response = new SRTRequest.ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));
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
