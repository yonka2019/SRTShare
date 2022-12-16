using PcapDotNet.Packets.Ethernet;
using SRTLibrary;
using System.Net;

namespace Server
{
    public class SRTSocket
    {
        public SAddress SocketAddress { get; } // address & port
        public MacAddress MacAddress { get; }
        public KeepAliveManager KeepAlive { get; }


        public SRTSocket(SAddress socketAddress, MacAddress macAddress, KeepAliveManager kaManager)
        {
            SocketAddress = socketAddress;
            MacAddress = macAddress;
            KeepAlive = kaManager;
        }
    }
}
