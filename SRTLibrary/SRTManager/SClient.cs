using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;

namespace SRTLibrary
{
    public class SClient  // SRT Client object
    {
        public IpV4Address IPAddress { get; set; }
        public ushort Port { get; set; }
        public MacAddress MacAddress { get; set; }
        public uint SocketId { get; set; }
        public uint MTU { get; set; }

        public SClient(IpV4Address iPAddress, ushort port, MacAddress macAddress, uint socketId, uint MTU)
        {
            IPAddress = iPAddress;
            Port = port;
            MacAddress = macAddress;
            SocketId = socketId;
            this.MTU = MTU;
        }

        public override string ToString()  // combine to "IPAddress:Port" 
        {
            return IPAddress.ToString() + Port.ToString();
        }
    }
}
