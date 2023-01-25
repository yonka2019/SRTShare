using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.ProtocolFields.Control;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Server
{
    internal class Program
    {
        internal const uint SERVER_SOCKET_ID = 123;
        internal static Dictionary<uint, SRTSocket> SRTSockets = new Dictionary<uint, SRTSocket>();

        internal static int screenIndex = 0;
        internal static Screen[] screens = Screen.AllScreens;
        internal static int screens_amount = 0;


        private static void Main()
        {
            screens_amount = screens.Length;

            Thread keyListenerThread = new Thread(ListenForKeys);
            keyListenerThread.Start();

            CConsole.WriteLine("\t-- SRT Server  --\n", MessageType.txtWarning);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;  // to handle libraries missing
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CtrlCKeyPressed);  // to handle server shutdown (ONLY CTRL + C)

            _ = ConfigManager.IP;

            new Thread(() => { PacketManager.ReceivePackets(0, HandlePacket); }).Start(); // always listen for any new connections

            // when server is being shutdown with the CTRL+C (break) the server will
            // have couple of seconds to send a "shutdown" message to the clients to notify them
            CConsole.WriteLine("[!] To shutdown server use only CTRL + C\n", MessageType.txtError);

            NetworkManager.PrintInterfaceData();
            NetworkManager.PrintServerData();

            keyListenerThread.Join();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            if (ex is FileNotFoundException || ex.InnerException is FileNotFoundException)
            {
                CConsole.WriteLine("[ERROR] File PcapDotNet.Core.dll couldn't be found or one of its dependencies. Make sure you have installed:\n" +
                    "- .NET Framework 4.5\n" +
                    "- WinPcap\n" +
                    "- Microsoft Visual C++ 2013..\n", MessageType.txtError);

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
                if (packet.Ethernet.Arp.TargetProtocolIpV4Address.ToString() == NetworkManager.LocalIp)  // the arp was for the server
                    RequestsHandler.HandleArp(packet);
            }
        }

        /// <summary>
        /// If a client lost connection, this function will be called
        /// </summary>
        /// <param name="socket_id">socket id who lost connection</param>
        internal static void LostConnection(uint socket_id)
        {
            CConsole.WriteLine($"[Keep-Alive] {SRTSockets[socket_id].SocketAddress.IPAddress} is dead, disposing resources..\n", MessageType.bgError);
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
                CConsole.WriteLine($"[Server] Client [{removedIp}] was removed\n", MessageType.txtError);
            }
            else
                CConsole.WriteLine($"[Server] Client [{SRTSockets[client_id].SocketAddress.IPAddress}] wasn't found\n", MessageType.txtError);
        }

        /// <summary>
        /// This function executes when the server turned off (ONLY CTRL + C)
        /// </summary>
        static void Console_CtrlCKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            CConsole.WriteLine("[Server] Shutting down...", MessageType.bgError);

            foreach (SRTSocket socket in SRTSockets.Values)  // send to each client shutdown message
            {
                ShutdownRequest shutdown_request = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, socket.SocketAddress.MacAddress.ToString(), NetworkManager.LocalIp, socket.SocketAddress.IPAddress.ToString(), ConfigManager.PORT, socket.SocketAddress.Port));
                Packet shutdown_packet = shutdown_request.Shutdown();
                PacketManager.SendPacket(shutdown_packet);
            }
            Environment.Exit(Environment.ExitCode);
        }


        static void form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                if(screenIndex + 1 < screens_amount)
                {
                    screenIndex++;
                }
            }

            else if (e.KeyCode == Keys.Left)
            {
                if (screenIndex > 0)
                {
                    screenIndex--;
                }
            }
        }

        static void ListenForKeys()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (screenIndex > 0)
                    {
                        screenIndex--;
                        CConsole.WriteLine($"Screen {screenIndex + 1} is shared.", MessageType.txtInfo);
                    }

                    else
                    {
                        CConsole.WriteLine($"You can only move between ({1} - {screens_amount}) screens", MessageType.txtWarning);
                    }

                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (screenIndex + 1 < screens_amount)
                    {
                        screenIndex++;
                        CConsole.WriteLine($"Screen {screenIndex + 1} is shared.", MessageType.txtInfo);
                    }

                    else
                    {
                        CConsole.WriteLine($"You can only move between ({1} - {screens_amount}) screens", MessageType.txtWarning);
                    }
                }
            }
        }
    }
}
