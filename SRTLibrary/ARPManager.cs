using PcapDotNet.Base;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;

namespace SRTLibrary
{
    public class ARPManager
    {
        /// <summary>
        /// The function builds a "mac request" packet
        /// </summary>
        /// <param name="my_device">Device to send to</param>
        /// <param name="to_ip">Ip to get its related mac</param>
        /// <returns>Mac request packet</returns>
        public static Packet Request(string to_ip, out bool sameSubnet)
        {
            sameSubnet = new IpV4Address(to_ip).IsInSubnet(PacketManager.Mask, PacketManager.DefaultGateway);
            // operate by if it not in the same subnet (get gateway mac, send to gatway.. etc..)

            if (!sameSubnet)
                to_ip = PacketManager.DefaultGateway;  // not in the same gateway -> get getway mac (via arp) and send the packet to him.
            

            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(PacketManager.MacAddress),
                Destination = new MacAddress("FF:FF:FF:FF:FF:FF"), // send to all
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.IpV4,  // IMPORTANT! NOT EthernetType.Arp HERE!!
                SenderHardwareAddress = new MacAddress(PacketManager.MacAddress).ToBytes().AsReadOnly(),
                SenderProtocolAddress = new IpV4Address(PacketManager.LocalIp).ToBytes().AsReadOnly(),
                TargetHardwareAddress = new MacAddress("FF:FF:FF:FF:FF:FF").ToBytes().AsReadOnly(), // send to all
                TargetProtocolAddress = new IpV4Address(to_ip).ToBytes().AsReadOnly(), // ip to get its related mac
                Operation = ArpOperation.Request,
            });
        }

        /// <summary>
        /// The function builds a "mac response" packet
        /// </summary>
        /// <param name="my_device">Device to send to</param>
        /// <param name="to_mac">Mac to send to</param>
        /// <param name="to_ip">Ip to send to</param>
        /// <returns></returns>
        public static Packet Reply(string to_mac, string to_ip)
        {
            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(PacketManager.MacAddress),
                Destination = new MacAddress(to_mac),
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.IpV4,  // IMPORTANT! NOT EthernetType.Arp HERE!!
                SenderHardwareAddress = new MacAddress(PacketManager.MacAddress).ToBytes().AsReadOnly(),
                SenderProtocolAddress = new IpV4Address(ConnectionConfig.SERVER_IP).ToBytes().AsReadOnly(),
                TargetHardwareAddress = new MacAddress(to_mac).ToBytes().AsReadOnly(), // mac to send to
                TargetProtocolAddress = new IpV4Address(to_ip).ToBytes().AsReadOnly(), // ip to send to
                Operation = ArpOperation.Reply,
            });
        }
    }
}
