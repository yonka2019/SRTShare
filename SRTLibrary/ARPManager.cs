using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SRTLibrary
{
    public class ARPManager
    {
        public static Packet Request(LivePacketDevice my_device, string to_ip)
        {
            string myMac = my_device.GetMacAddress().ToString();
            //bool sameSubnet = new IPAddress(new IpV4Address(to_ip).ToBytes()).IsInSubnet(PacketManager.mask);

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
                SenderHardwareAddress = new MacAddress(myMac).ToBytes().AsReadOnly(),
                SenderProtocolAddress = new IpV4Address("127.0.0.1").ToBytes().AsReadOnly(),
                TargetHardwareAddress = new MacAddress("FF:FF:FF:FF:FF:FF").ToBytes().AsReadOnly(),
                TargetProtocolAddress = new IpV4Address(to_ip).ToBytes().AsReadOnly(),
                Operation = ArpOperation.Request,
            });
        }

        public static Packet Reply(LivePacketDevice my_device, string to_mac, string to_ip)
        {
            string myMac = my_device.GetMacAddress().ToString();

            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(myMac),
                Destination = new MacAddress(to_mac),
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.Arp,
                SenderHardwareAddress = new MacAddress(myMac).ToBytes().AsReadOnly(),
                SenderProtocolAddress = new IpV4Address("192.168.1.29").ToBytes().AsReadOnly(),
                TargetHardwareAddress = new MacAddress(to_mac).ToBytes().AsReadOnly(),
                TargetProtocolAddress = new IpV4Address(to_ip).ToBytes().AsReadOnly(),
                Operation = ArpOperation.Reply,
            });
        }
    }
}
