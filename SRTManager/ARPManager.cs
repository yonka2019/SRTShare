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
        public static Packet Request(LivePacketDevice my_device, string to_ip)
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
                SenderProtocolAddress = new SAddress("127.0.0.1").GetIpByted().AsReadOnly(),
                TargetHardwareAddress = Encoding.ASCII.GetBytes("FF:FF:FF:FF:FF:FF").AsReadOnly(),
                TargetProtocolAddress = new SAddress(to_ip).GetIpByted().AsReadOnly(),
                Operation = ArpOperation.Request,
            });
        }

        public static Packet Reply(LivePacketDevice my_device, ReadOnlyCollection<byte> to_mac, string to_ip)
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
                SenderProtocolAddress = new SAddress("127.0.0.1").GetIpByted().AsReadOnly(),
                TargetHardwareAddress = to_mac,
                TargetProtocolAddress = new SAddress(to_ip).GetIpByted().AsReadOnly(),
                Operation = ArpOperation.Reply,
            });
        }
    }
}
