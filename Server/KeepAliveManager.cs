using PcapDotNet.Packets;
using SRTLibrary;
using SRTLibrary.SRTManager.RequestsFactory;
using System.Threading;
using System.Timers;
using SRTRequest = SRTLibrary.SRTManager.RequestsFactory;

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
                //LostConnection.Invoke(client.SocketId);
                System.Console.WriteLine($"[{client.SocketId}] is dead");
                connected = false;

                timer.Stop();
                timer.Dispose();
            }
        }

        internal void StartCheck()
        {
            Thread kaChecker = new Thread(new ParameterizedThreadStart(KeepAliveChecker));  // create thread of keep-alive checker

            kaChecker.Start(client.SocketId);
        }

        internal void ConfirmStatus()  // reset timeout seconds
        {
            timeoutSeconds = 0;
            System.Console.WriteLine($"[{client.SocketId}] is still alive");
        }

        internal void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;
            timer.Start();

            while (connected)
            {
                KeepAliveRequest keepAlive_request = new SRTRequest.KeepAliveRequest
                                (PacketManager.BuildBaseLayers(PacketManager.macAddress, client.MacAddress.ToString(), PacketManager.localIp, client.IPAddress.ToString(), PacketManager.SERVER_PORT, client.Port));

                Packet keepAlive_packet = keepAlive_request.Check(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);

                Thread.Sleep(3000);  // 1 second wait
            }
        }
    }
}

