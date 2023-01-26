using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;

namespace SRTShareLib
{
    public class SClient  // SRT Client object
    {
        public IpV4Address IPAddress { get; private set; }
        public ushort Port { get; private set; }
        public MacAddress MacAddress { get; private set; }
        public uint SocketId { get; private set; }
        public uint MTU { get; private set; }
        public ushort EncryptionMethod { get; private set; }

        public SClient(IpV4Address iPAddress, ushort port, MacAddress macAddress, uint socketId, uint MTU, ushort encryption)
        {
            IPAddress = iPAddress;
            Port = port;
            MacAddress = macAddress;
            SocketId = socketId;
            this.MTU = MTU;
            EncryptionMethod = encryption;
        }

        public override string ToString()  // combine to "IPAddress:Port" 
        {
            return IPAddress.ToString() + Port.ToString();
        }
    }
}
