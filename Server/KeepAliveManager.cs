using PcapDotNet.Packets;
using SRTLibrary;
using SRTLibrary.SRTManager.RequestsFactory;
using System.Threading;
using System.Timers;

namespace Server
{
    internal class KeepAliveManager
    {
        private readonly SClient client;
        private int timeoutSeconds;
        private bool connected;

        private static System.Timers.Timer timer;

        internal delegate void Notify(uint socket_id);
        internal event Notify LostConnection;

        internal KeepAliveManager(SClient client)
        {
            this.client = client;
            timeoutSeconds = 0;
            connected = true;

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutSeconds++;

            if (timeoutSeconds == 5)  // KEEP-ALIVE TIMED-OUT
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
            Thread kaChecker = new Thread(new ParameterizedThreadStart(KeepAliveChecker));  // create thread of keep-alive checker
            kaChecker.Start(client.SocketId);
        }

        /// <summary>
        /// The function confirms the keep-alive message (resets the timer)
        /// </summary>
        internal void ConfirmStatus()  // reset timeout seconds
        {
            timeoutSeconds = 0;
            System.Console.WriteLine($"[{Program.SRTSockets[client.SocketId].SocketAddress.IPAddress}] is still alive");
        }

        /// <summary>
        /// The function sends keep-alive packets every 3 seconds while the client is connected
        /// </summary>
        /// <param name="dest_socket_id"></param>
        internal void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;
            timer.Start();

            while (connected)
            {
                KeepAliveRequest keepAlive_request = new KeepAliveRequest
                                (PacketManager.BuildBaseLayers(PacketManager.MacAddress, client.MacAddress.ToString(), PacketManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

                Packet keepAlive_packet = keepAlive_request.Check(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);

                Thread.Sleep(3000);  // 3 second wait between the keep-alives
            }
        }
    }
}
