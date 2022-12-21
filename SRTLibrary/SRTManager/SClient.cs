using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;

namespace SRTLibrary
{
    public class SClient
    {
        public IpV4Address IPAddress { get; set; }
        public ushort Port { get; set; }
        public MacAddress MacAddress { get; set; }
        public uint SocketId { get; set; }

        public SClient(IpV4Address iPAddress, ushort port, MacAddress macAddress, uint socketId)
        {
            IPAddress = iPAddress;
            Port = port;
            MacAddress = macAddress;
            SocketId = socketId;
        }


        public override string ToString()  // combine to "IPAddress:Port" 
        {
            return IPAddress.ToString() + Port.ToString();
        }
    }
}
