using PcapDotNet.Packets.Ethernet;
using SRTLibrary;
using System.Net;

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
