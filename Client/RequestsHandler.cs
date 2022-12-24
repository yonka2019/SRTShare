using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTLibrary;
using Control = SRTLibrary.SRTManager.ProtocolFields.Control;
using Data = SRTLibrary.SRTManager.ProtocolFields.Data;

using SRTLibrary.SRTManager.RequestsFactory;
using System;
using System.Windows.Forms;
using SRTRequest = SRTLibrary.SRTManager.RequestsFactory;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace Client
{
    internal class RequestsHandler
    {
        private static readonly List<byte> all_chuncks = new List<byte>();


        /// <summary>
        /// The function handles what happens after getting an induction message from the server
        /// </summary>
        /// <param name="handshake_request">Handshake object</param>
        internal static void HandleInduction(Control.Handshake handshake_request)
        {
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(PacketManager.localIp, MainView.myPort, DateTime.Now))
            {
                HandshakeRequest handshake_response = new HandshakeRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, MainView.server_mac, PacketManager.localIp, PacketManager.SERVER_IP, MainView.myPort, PacketManager.SERVER_PORT));

                // client -> server (conclusion)
                IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
                Console.WriteLine("My ip string: " + PacketManager.localIp);
                Console.WriteLine("My ip address: " + peer_ip.ToString());

                Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: true, MainView.client_socket_id, handshake_request.SOCKET_ID, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                PacketManager.SendPacket(handshake_packet);
            }
            else
            {
                // Exit the prgram and send a shutdwon request
                ShutDownRequest shutdown_response = new ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.macAddress, MainView.server_mac, PacketManager.localIp, PacketManager.SERVER_IP, MainView.myPort, PacketManager.SERVER_PORT));
                Packet shutdown_packet = shutdown_response.Exit();
                PacketManager.SendPacket(shutdown_packet);

                MessageBox.Show("Wrong cookie - Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// The function handles what happens after getting an arp message from the server
        /// </summary>
        /// <param name="server_mac">Server's mac</param>
        /// <param name="myPort">Client's port</param>
        /// <param name="client_socket_id">Client's socket id</param>
        internal static void HandleArp(string server_mac, ushort myPort, uint client_socket_id)
        {
            HandshakeRequest handshake = new HandshakeRequest
                    (PacketManager.BuildBaseLayers(PacketManager.macAddress, server_mac, PacketManager.localIp, PacketManager.SERVER_IP, myPort, PacketManager.SERVER_PORT));

            //create induction packet
            DateTime now = DateTime.Now;

            IpV4Address peer_ip = new IpV4Address(PacketManager.localIp);
            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.localIp, myPort, now), init_psn: 0, p_ip: peer_ip, clientSide: true, client_socket_id, 0);

            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleData(Data.SRTHeader data_request, PictureBox pictureBox)
        {
            if(data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)
            {
                all_chuncks.AddRange(data_request.DATA);
                ShowImage(true, pictureBox);
                all_chuncks.Clear();
            }

            else
                all_chuncks.AddRange(data_request.DATA);
        }

        private static void ShowImage(bool allChunksReceived, PictureBox pictureBox)
        {

            if (allChunksReceived)
                Console.WriteLine("[IMAGE BUILT SUCCESSFULLY] SHOWING IMAGE\n--------------------\n\n\n");
            else
                Console.WriteLine("[CHUNKS MISSING] SHOWING IMAGE\n--------------------\n\n\n");

            using (MemoryStream ms = new MemoryStream(all_chuncks.ToArray()))
            {
                pictureBox.Image = new Bitmap(Image.FromStream(ms));
            }
        }

    }
}
