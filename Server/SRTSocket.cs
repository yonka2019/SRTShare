using SRTShareLib;

namespace Server
{
    internal class SRTSocket
    {
        internal SClient SocketAddress { get; private set; } // address & port
        internal KeepAliveManager KeepAlive { get; private set; }
        internal VideoManager Data { get; private set; }

        internal SRTSocket(SClient socketAddress, KeepAliveManager kaManager, VideoManager dataManager)
        {
            SocketAddress = socketAddress;
            KeepAlive = kaManager;
            Data = dataManager;
        }
    }
}
