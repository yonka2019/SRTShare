using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using System;
using System.Collections.Generic;
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
                if (packet.Ethernet.Arp.TargetProtocolIpV4Address.ToString() == PacketManager.SERVER_IP)  // the arp was for the server
                {
                    RequestsHandler.HandleArp(packet);
                }
            }
        }

        internal static void LostConnection(uint socket_id)
        {
            Console.WriteLine($"[{socket_id}] is dead");
            SRTSockets[socket_id].Data.StopVideo();
        }
    }
}