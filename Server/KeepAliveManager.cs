using PcapDotNet.Packets;
using SRTManager;
using System.Threading;

using SRTRequest = SRTManager.RequestsFactory;

namespace Server
{
    public class KeepAliveManager
    {
        private readonly uint socket_id;
        private readonly ushort socket_port;
        private int timeoutSeconds;

        public delegate void Notify(uint socket_id);
        public event Notify LostConnection;


        public KeepAliveManager(uint socket_id, ushort socket_port)
        {
            this.socket_id = socket_id;
            this.socket_port = socket_port;
            timeoutSeconds = 0;
        }

        public void StartCheck()
        {
            Thread kaChecker = new Thread(new ParameterizedThreadStart(KeepAliveChecker));  // create thread of keep-alive checker

            kaChecker.Start(socket_id);
        }

        public void ConfirmStatus()
        {
            SetTimeoutTime(0); // add stopwatch and etc
        }

        private void SetTimeoutTime(int seconds)
        {
            timeoutSeconds = seconds;
        }

        private void KeepAliveChecker(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;

            while (true)
            {
                SRTRequest.KeepAliveRequest keepAlive_request = new SRTRequest.KeepAliveRequest
                                (PacketManager.BuildBaseLayers(PacketManager.SERVER_PORT, socket_port));

                Packet keepAlive_packet = keepAlive_request.Check(u_dest_socket_id);
                PacketManager.SendPacket(keepAlive_packet);
            }
        }
    }

}

