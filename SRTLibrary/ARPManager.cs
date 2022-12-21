using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
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
        public static Packet Request(LivePacketDevice my_device, string to_ip)
        {
            string myMac = GetMyMac(my_device);
            //bool sameSubnet = new IPAddress(new IpV4Address(to_ip).ToBytes()).IsInSubnet(PacketManager.mask);

            return PacketBuilder.Build(
            DateTime.Now,
            new EthernetLayer
            {
                Source = new MacAddress(myMac),
                Destination = new MacAddress("FF:FF:FF:FF:FF:FF"), // send to all
                EtherType = EthernetType.Arp
            },

            new ArpLayer
            {
                ProtocolType = EthernetType.Arp,
                SenderHardwareAddress = new MacAddress(myMac).ToBytes().AsReadOnly(),
                SenderProtocolAddress = new IpV4Address("127.0.0.1").ToBytes().AsReadOnly(),
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
                SenderProtocolAddress = new IpV4Address(PacketManager.SERVER_IP).ToBytes().AsReadOnly(),
                TargetHardwareAddress = new MacAddress(to_mac).ToBytes().AsReadOnly(), // mac to send to
                TargetProtocolAddress = new IpV4Address(to_ip).ToBytes().AsReadOnly(), // ip to send to
                Operation = ArpOperation.Reply,
            });
        }


        /// <summary>
        /// The function gets the mac of the given device
        /// </summary>
        /// <param name="my_device">device to check</param>
        /// <returns>Mac of the device</returns>
        public static string GetMyMac(LivePacketDevice my_device)
        {
            return my_device.GetMacAddress().ToString();
        }
    }
}
