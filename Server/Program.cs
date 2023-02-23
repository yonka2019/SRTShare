using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
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
        public static Dictionary<uint, SRTSocket> SRTSockets = new Dictionary<uint, SRTSocket>();

        public static int SharedScreenIndex { get; private set; }
        private static readonly Screen[] screens = Screen.AllScreens;

        private static Thread pressedKeyListenerT;
        private static Thread handlePackets;

        private static void Main()
        {
            CConsole.WriteLine("\t-- SRT Server  --\n", MessageType.txtWarning);

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;  // to handle libraries missing

            _ = ConfigManager.IP;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CtrlCKeyPressed);  // to handle server shutdown (ONLY CTRL + C)

            // always listen for any new connections
            handlePackets = new Thread(() => { PacketManager.ReceivePackets(0, HandlePacket); });
            handlePackets.Start();

            PrintGreeting();

            NetworkManager.PrintInterfaceData();
            NetworkManager.PrintServerData();

            // server started up - no errors, handle key press (switch shared screen feature)
            pressedKeyListenerT = new Thread(KeyPressedListener);
            pressedKeyListenerT.Start();

            CConsole.WriteLine("[Server] UP\n", MessageType.txtSuccess);
        }

        /// <summary>
        /// Prints greeting to server console
        /// </summary>
        private static void PrintGreeting()
        {
            // when server is being shutdown with the CTRL+C (break) the server will
            // have couple of seconds to send a "shutdown" message to the clients to notify them
            CConsole.WriteLine("[!] To shutdown server use only CTRL + C\n", MessageType.txtError);

            CConsole.WriteLine("[*] You can switch between the screens with the [<-] [->] keys arrow\n", MessageType.txtInfo);
        }

        /// <summary>
        /// Handle unhandled exception (especially libraries missing)
        /// </summary>
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
                            SRTSockets[handshake_request.SOURCE_SOCKET_ID].KeepAlive.StartCheck();  // start keep-alive checking
                            SRTSockets[handshake_request.SOURCE_SOCKET_ID].Data.StartVideo();  // start keep-alive checking
                        }
                    }
                    else if (KeepAlive.IsKeepAlive(payload))  // (SRT) KeepAlive
                    {
                        KeepAlive keepAlive = new KeepAlive(payload);

                        if (SRTSockets.ContainsKey(keepAlive.SOURCE_SOCKET_ID))
                            SRTSockets[keepAlive.SOURCE_SOCKET_ID].KeepAlive.ConfirmStatus();  // sign as alive
                    }
                    else if (QualityUpdate.IsQualityUpdate(payload))  // (SRT) QualityUpdate - update the quality especially to this client
                    {
                        QualityUpdate qualityUpdate = new QualityUpdate(payload);

                        Console.WriteLine($"[Quality Update] {SRTSockets[qualityUpdate.SOURCE_SOCKET_ID].SocketAddress.IPAddress} updated quality: {qualityUpdate.QUALITY}%\n");

                        SRTSockets[qualityUpdate.SOURCE_SOCKET_ID].Data.CurrentQuality = qualityUpdate.QUALITY;

                    }
                    else if (NAK.IsNAK(payload))  // (SRT) NAK
                    {
                        NAK nak_request = new NAK(payload);
                        List<uint> missingSequenceNumbers = nak_request.LOST_PACKETS;

                        // resend all the packets for each missing sequence number (each image)

                    }
                    else if (ACK.IsACK(payload))  // (SRT) ACK
                    {
                        ACK ack_request = new ACK(payload);
                        uint receivedImageSequenceNumber = ack_request.ACK_SEQUENCE_NUMBER;

                        // clear all the packets of teh received image sequence number
                    }

                    else if (Shutdown.IsShutdown(payload))  // (SRT) Shutdown
                        RequestsHandler.HandleShutDown(packet);
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
        internal static void Client_LostConnection(uint socket_id)
        {
            SClient clientSocket = SRTSockets[socket_id].SocketAddress;

            CConsole.WriteLine($"[Keep-Alive] {clientSocket.IPAddress} is dead, disposing resources..\n", MessageType.bgError);
            DisposeClient(socket_id);
        }

        /// <summary>
        /// On lost connection / shutdown, we need to dispose client resources and information
        /// </summary>
        /// <param name="client_id">client id who need to be cleaned</param>
        internal static void DisposeClient(uint client_id)
        {
            if (SRTSockets.ContainsKey(client_id))
            {
                SClient clientSocket = SRTSockets[client_id].SocketAddress;

                SRTSockets[client_id].Data.StopVideo();
                SRTSockets[client_id].KeepAlive.Disable();

                string removedClientIP = $"{clientSocket.IPAddress}";

                SRTSockets.Remove(client_id);
                CConsole.WriteLine($"[Server] Client [{removedClientIP}] was removed\n", MessageType.txtWarning);
            }
            else
                CConsole.WriteLine($"[Server] Client ID [{client_id}] doesn't exist\n", MessageType.txtError);
        }

        /// <summary>
        /// This function executes when the server turned off (ONLY CTRL + C)
        /// Safely disposes all used resources, and sends shutdown broadcast to all connected clients
        /// </summary>
        private static void Console_CtrlCKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            CConsole.Write("[Server] Shutting down...", MessageType.bgError);
            CConsole.WriteLine(" (Wait approx. ~5 seconds)", MessageType.txtError);
            pressedKeyListenerT.Abort();

            foreach (uint socketId in SRTSockets.Keys)  // send to each client shutdown message
            {
                SRTSocket socket = SRTSockets[socketId];

                ShutdownRequest shutdown_request = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, socket.SocketAddress.MacAddress.ToString(), NetworkManager.LocalIp, socket.SocketAddress.IPAddress.ToString(), ConfigManager.PORT, socket.SocketAddress.Port));
                Packet shutdown_packet = shutdown_request.Shutdown(socketId, SERVER_SOCKET_ID, IsInVideoStage(socketId), GetSocketPeerEncryption(socketId));
                PacketManager.SendPacket(shutdown_packet);

                SRTSockets[socketId].Data.StopVideo();
                SRTSockets[socketId].KeepAlive.Disable();
            }

            handlePackets.Abort();

            Environment.Exit(0);
        }

        /// <summary>
        /// Listenes the key press, for feature to switch between the screens via the -> <- buttons
        /// </summary>
        private static void KeyPressedListener()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (SharedScreenIndex > 0)
                    {
                        SharedScreenIndex--;
                        CConsole.WriteLine($"[Server] Screen {SharedScreenIndex + 1} is shared\n", MessageType.txtInfo);
                    }
                    else
                    {
                        CConsole.WriteLine($"[Server] You can only switch between ({1} - {screens.Length}) screens\n", MessageType.txtWarning);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (SharedScreenIndex + 1 < screens.Length)
                    {
                        SharedScreenIndex++;
                        CConsole.WriteLine($"[Server] Screen {SharedScreenIndex + 1} is shared\n", MessageType.txtInfo);
                    }
                    else
                    {
                        CConsole.WriteLine($"[Server] You can only switch between ({1} - {screens.Length}) screens\n", MessageType.txtWarning);
                    }
                }
            }
        }

        /// <summary>
        /// returns true if the given socket id (client) in the video stage (server sending data)
        /// </summary>
        /// <param name="socketId">socket id (client)</param>
        private static bool IsInVideoStage(uint socketId)
        {
            return SRTSockets[socketId].Data.VideoStage;
        }

        /// <summary>
        /// returns the selected encryption type by the client at the handshake part
        /// </summary>
        /// <param name="socketId">socket id (client)</param>
        /// <returns>chosen encryption method</returns>
        private static PeerEncryptionData GetSocketPeerEncryption(uint socketId)
        {
            return SRTSockets[socketId].Data.PeerEncryption;
        }
    }
}