using SRTLibrary;

namespace Server
{
    internal class SRTSocket
    {
        internal SClient SocketAddress { get; } // address & port
        internal KeepAliveManager KeepAlive { get; }


        internal SRTSocket(SClient socketAddress, KeepAliveManager kaManager)
        {
            SocketAddress = socketAddress;
            KeepAlive = kaManager;
        }
    }
}
