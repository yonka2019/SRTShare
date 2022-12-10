using PcapDotNet.Packets;
using SRTManager;
using System.Threading;
using System.Timers;

using SRTRequest = SRTManager.RequestsFactory;

namespace Server
{
    public class KeepAliveManager
    {
        private readonly uint socket_id;
        private readonly ushort socket_port;
        private int timeoutSeconds;
        private bool connected;

        private static System.Timers.Timer timer;

        public delegate void Notify(uint socket_id);
        public event Notify LostConnection;


        public KeepAliveManager(uint socket_id, ushort socket_port)
        {
            this.socket_id = socket_id;
            this.socket_port = socket_port;
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
                LostConnection.Invoke(socket_id);
                connected = false;

                timer.Stop();
                timer.Dispose();
            }
        }

        public void StartCheck()
        {
            Thread kaChecker = new Thread(new ParameterizedThreadStart(KeepAliveChecker));  // create thread of keep-alive checker

            kaChecker.Start(socket_id);
        }

        public void ConfirmStatus()  // reset timeout seconds
        {
            timeoutSeconds = 0;
        }

        private void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;
            timer.Start();

            while (connected)
            {
                SRTRequest.KeepAliveRequest keepAlive_request = new SRTRequest.KeepAliveRequest
                                (PacketManager.BuildBaseLayers(PacketManager.SERVER_PORT, socket_port));

                Packet keepAlive_packet = keepAlive_request.Check(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);

                Thread.Sleep(1000);  // 1 second wait
            }
        }
    }
}

