using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTManager
{
    public class ARPManager
    {
        public static Packet Request(LivePacketDevice my_device, ReadOnlyCollection<byte> to_ip)
        {
            string myMac = my_device.GetMacAddress().ToString();

            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(myMac),
                Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.Arp,
                SenderHardwareAddress = Encoding.ASCII.GetBytes(myMac).AsReadOnly(),
                SenderProtocolAddress = Encoding.ASCII.GetBytes("127.0.0.1").AsReadOnly(),
                TargetHardwareAddress = null,
                TargetProtocolAddress = to_ip,
                Operation = ArpOperation.Request,
            });
        }

        public static Packet Reply(LivePacketDevice my_device, ReadOnlyCollection<byte> to_mac, ReadOnlyCollection<byte> to_ip)
        {
            string myMac = my_device.GetMacAddress().ToString();

            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(myMac),
                Destination = new MacAddress(to_mac.ToString()),
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.Arp,
                SenderHardwareAddress = Encoding.ASCII.GetBytes(myMac).AsReadOnly(),
                SenderProtocolAddress = Encoding.ASCII.GetBytes("127.0.0.1").AsReadOnly(),
                TargetHardwareAddress = to_mac,
                TargetProtocolAddress = to_ip,
                Operation = ArpOperation.Reply,
            });
        }
    }
}
