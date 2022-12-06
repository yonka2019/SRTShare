using System.Net;

namespace Server
{
    public class SRTSocket
    {
        public SRTSocket(IPEndPoint ipEP, KeepAliveManager kaManager)
        {
            IPEP = ipEP;
            KeepAlive = kaManager;
        }

        public KeepAliveManager KeepAlive { get; }
        public IPEndPoint IPEP { get; }  // address & port
    }
}
