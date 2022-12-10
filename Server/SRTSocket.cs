using SRTManager;
using System.Net;

namespace Server
{
    public class SRTSocket
    {
        public KeepAliveManager KeepAlive { get; }
        public SAddress SocketAddress { get; } // address & port

        public SRTSocket(SAddress socketAddress, KeepAliveManager kaManager)
        {
            SocketAddress = socketAddress;
            KeepAlive = kaManager;
        }
    }
}
