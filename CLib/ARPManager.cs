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
using System.Text;
using System.Threading.Tasks;

namespace CLib
{
    public class ARPManager
    {
        public static Packet Request(LivePacketDevice my_device, string to_ip)
        {
            string myMac = my_device.GetMacAddress().ToString();

            byte[] b_spa = BitConverter.GetBytes(new IpV4Address("127.0.0.1").ToValue());
            if (BitConverter.IsLittleEndian)
            Array.Reverse(b_spa);
            ReadOnlyCollection<byte> spa = b_spa.AsReadOnly();

            // need to do function for this three lines and do for mac and ipv4 respectively
            // CHANGE ALL UINT ADDRESEES TO IPV4 ADDRESS THINK ABOUT THATT

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
                SenderHardwareAddress = BitConverter.GetBytes(new MacAddress(myMac).ToValue()).AsReadOnly(),
                SenderProtocolAddress = spa,
                TargetHardwareAddress = BitConverter.GetBytes(new MacAddress("FF:FF:FF:FF:FF:FF".Reverse()).ToValue()).AsReadOnly(),
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
