using SRTLibrary;

namespace Server
{
    internal class SRTSocket
    {
        internal SClient SocketAddress { get; } // address & port
        internal KeepAliveManager KeepAlive { get; }
        internal VideoManager Data { get; }

        internal SRTSocket(SClient socketAddress, KeepAliveManager kaManager, VideoManager dataManager)
        {
            SocketAddress = socketAddress;
            KeepAlive = kaManager;
            Data = dataManager;
        }
    }
}
