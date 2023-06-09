﻿using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.RequestsFactory;
using System.Threading;
using System.Timers;

namespace Server.Managers
{
    internal class KeepAliveManager : IManager
    {
        private const int TIMEOUT_SECONDS = 10;
        private const int KA_REFRESH_SECONDS = 5;

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

        /// <summary>
        /// The function starts the thread responsible for keep-alive sending
        /// </summary>
        public void Start()
        {
            kaChecker = new Thread(new ParameterizedThreadStart(Run));  // create thread of keep-alive checker
            kaChecker.Start(client.SocketId);
        }

        public void Stop()
        {
            timer.Stop();
            timer.Dispose();
            kaChecker.Abort();
        }

        /// <summary>
        /// The function sends keep-alive packets every 3 seconds while the client is connected  [not encrypted, even it in video stage]
        /// </summary>
        /// <param name="dest_socket_id"></param>
        private void Run(object dest_socket_id)  // Keep Alive Checker
        {
            uint u_dest_socket_id = (uint)dest_socket_id;
            timer.Start();

            while (connected)
            {
                KeepAliveRequest keepAlive_request = new KeepAliveRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, client.MacAddress.ToString(), NetworkManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

                Packet keepAlive_packet = keepAlive_request.Alive(u_dest_socket_id, Program.SERVER_SOCKET_ID);
                PacketManager.SendPacket(keepAlive_packet);

                Thread.Sleep(KA_REFRESH_SECONDS * 1000);
            }
        }

        /// <summary>
        /// The function confirms the keep-alive message (resets the timer)
        /// </summary>
        internal void ConfirmStatus()  // reset timeout seconds
        {
            timeoutSeconds = 0;
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
    }
}
