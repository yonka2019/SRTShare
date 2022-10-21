﻿using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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
        private static PacketCommunicator communicator;
        private static PacketDevice selectedDevice;
        private readonly Thread pRecvThread;

        private const string DEFAULT_INTERFACE_SUBSTRING = "Intel"; // default interface must contain this substring to be automatically chosen

        public MainView()
        {
            InitializeComponent();
            pRecvThread = new Thread(new ThreadStart(RecvP));
            SetPacketDevice();

            // start the capture
            pRecvThread.Start();
        }
        private static void SetPacketDevice()
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int deviceIndex = -1;

            if (allDevices.Count == 0)
                return;

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                if (device.Description != null)
                {
                    if (device.Description.Contains(DEFAULT_INTERFACE_SUBSTRING))
                    {
                        deviceIndex = i + 1;
                        break;
                    }
                }
            }

            // Take the selected adapter
            selectedDevice = allDevices[deviceIndex - 1];
            Console.WriteLine($"[!] SELECTED INTERFACE: {selectedDevice.Description}");
        }

        private void RecvP()
        {
            // open the device
            using (communicator =
            selectedDevice.Open(65536,                         // portion of the packet to capture
                                                               // 65536 guarantees that the whole packet will be captured on all the link layers
                    PacketDeviceOpenAttributes.Promiscuous,  // promiscuous mode
                    1000))                                  // read timeout
            {
                Console.WriteLine("[LISTENING] " + selectedDevice.Description + "...");
                communicator.ReceivePackets(0, PacketHandler);
            }
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private void PacketHandler(Packet packet)
        {
            UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
            if (datagram != null && datagram.SourcePort == 6969)
            {
                MemoryStream stream = datagram.Payload.ToMemoryStream();
                byte[] byteStream = stream.ToArray();

                // [0][1]
                current_packet_id = BitConverter.ToUInt16(byteStream, 0); // take first two bytes of the chunk --> ([ID (2 bytes)] <-- [CHUNKS NUMBER (2 bytes)][DATA]

                // [2][3]
                total_chunks_number = BitConverter.ToUInt16(byteStream, 2); // take second two bytes of the chunk ([ID (2 bytes)] --> [CHUNKS NUMBER (2 bytes)] <--[DATA]
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

        private void ShowImage(bool allChunksReceived)
        {
            Console.WriteLine($"[GOT] Image (Total chunks: {total_chunks_number})"); // each image

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
