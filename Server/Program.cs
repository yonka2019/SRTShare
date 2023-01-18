using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using SRTLibrary;
using SRTLibrary.SRTManager.ProtocolFields.Control;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _ = ConfigManager.IP;

            new Thread(() => { PacketManager.ReceivePackets(0, HandlePacket); }).Start(); // always listen for any new connections

            PacketManager.PrintInterfaceData();
            PacketManager.PrintServerData();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            if (ex is FileNotFoundException || ex.InnerException is FileNotFoundException)
            {
                Console.WriteLine("[ERROR] File PcapDotNet.Core.dll couldn't be found or one of its dependencies. Make sure you have installed:\n" +
                    "- .NET Framework 4.5\n" +
                    "- WinPcap\n" +
                    "- Microsoft Visual C++ 2013..\n");

                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Callback function invoked by Pcap.Net for every incoming packet
        /// </summary>
        /// <param name="packet">New given packet</param>
        private static void HandlePacket(Packet packet)
        {
            if (packet.IsValidUDP(ConfigManager.PORT))  // UDP Packet addressed to server
            {
                UdpDatagram datagram = packet.Ethernet.IpV4.Udp;
                byte[] payload = datagram.Payload.ToArray();

                if (SRTHeader.IsControl(payload))  // (SRT) Control
                {
                    if (Handshake.IsHandshake(payload))  // (SRT) Handshake
                    {
                        Handshake handshake_request = new Handshake(payload);

                        if (handshake_request.TYPE == (uint)Handshake.HandshakeType.INDUCTION)  // (SRT) Induction
                            RequestsHandler.HandleInduction(packet, handshake_request);

                        else if (handshake_request.TYPE == (uint)Handshake.HandshakeType.CONCLUSION)  // (SRT) Conclusion
                        {
                            RequestsHandler.HandleConclusion(packet, handshake_request);
                            SRTSockets[handshake_request.SOCKET_ID].KeepAlive.StartCheck();  // start keep-alive checking
                            SRTSockets[handshake_request.SOCKET_ID].Data.StartVideo();  // start keep-alive checking
                        }
                    }

                    else if (Shutdown.IsShutdown(payload))  // (SRT) Shutdown
                        RequestsHandler.HandleShutDown(packet);

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
                if (packet.Ethernet.Arp.TargetProtocolIpV4Address.ToString() == PacketManager.LocalIp)  // the arp was for the server
                    RequestsHandler.HandleArp(packet);
            }
        }

        /// <summary>
        /// If a client lost connection, this function will be called
        /// </summary>
        /// <param name="socket_id">socket id who lost connection</param>
        internal static void LostConnection(uint socket_id)
        {
            Console.WriteLine($"[Keep-Alive] {SRTSockets[socket_id].SocketAddress.IPAddress} is dead, disposing resources..\n");
            Dispose(socket_id);
        }

        /// <summary>
        /// On lost connection / shutdown, we need to dispose client resources and information
        /// </summary>
        /// <param name="client_id">client id who need to be cleaned</param>
        internal static void Dispose(uint client_id)
        {
            SRTSockets[client_id].Data.StopVideo();

            if (SRTSockets.ContainsKey(client_id))
            {
                string removedIp = SRTSockets[client_id].SocketAddress.IPAddress.ToString();

                SRTSockets.Remove(client_id);
                Console.WriteLine($"[Server] Client [{removedIp}] was removed\n");
            }
            else
                Console.WriteLine($"[Server] Client [{SRTSockets[client_id].SocketAddress.IPAddress}] wasn't found\n");
        }
    }
}