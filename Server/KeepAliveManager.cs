using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Threading;
using System.Timers;

using CConsole = SRTShareLib.CColorManager;

namespace Server
{
    internal class KeepAliveManager
    {
        private const int TIMEOUT_SECONDS = 5;
        private const int KA_REFRESH_SECONDS = 3;

        private readonly SClient client;
        private int timeoutSeconds;
        private bool connected;

        private readonly System.Timers.Timer timer;
        private Thread kaChecker;

        internal delegate void Notify(uint socket_id);
        internal event Notify LostConnection;

        internal KeepAliveManager(SClient client)
        {
            this.client = client;
            timeoutSeconds = 0;
            connected = true;

            timer = new System.Timers.Timer(1000);  // 1 second timeout
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutSeconds++;

            if (timeoutSeconds == TIMEOUT_SECONDS)  // KEEP-ALIVE TIMED-OUT
            {
                LostConnection.Invoke(client.SocketId);
                connected = false;

                timer.Stop();
                timer.Dispose();
            }
        }

        /// <summary>
        /// The function starts the thread responsible for keep-alive sending
        /// </summary>
        internal void StartCheck()
        {
            kaChecker = new Thread(new ParameterizedThreadStart(KeepAliveChecker));  // create thread of keep-alive checker
            kaChecker.Start(client.SocketId);
        }

        internal void Disable()
        {
            timer.Stop();
            timer.Dispose();
            kaChecker.Abort();
        }

        /// <summary>
        /// The function confirms the keep-alive message (resets the timer)
        /// </summary>
        internal void ConfirmStatus()  // reset timeout seconds
        {
            SClient clientSocket = Program.SRTSockets[client.SocketId].SocketAddress;

            timeoutSeconds = 0;
            CConsole.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Keep-Alive] {clientSocket.IPAddress}:{clientSocket.Port} is alive\n", MessageType.txtSuccess);
        }

        /// <summary>
        /// The function sends keep-alive packets every 3 seconds while the client is connected
        /// </summary>
        /// <param name="dest_socket_id"></param>
        private void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;
            timer.Start();

            while (connected)
            {
                KeepAliveRequest keepAlive_request = new KeepAliveRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, client.MacAddress.ToString(), NetworkManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

                Packet keepAlive_packet = keepAlive_request.Alive(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);

                Thread.Sleep(KA_REFRESH_SECONDS * 1000);
            }
        }
    }
}
