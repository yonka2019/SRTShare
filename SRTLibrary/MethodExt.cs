using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;

namespace SRTLibrary
{
    public static class MethodExt
    {
        /// <summary>
        /// The function reverses the given ip
        /// </summary>
        /// <param name="ip">Ip to reverse</param>
        /// <returns>Reversed ip</returns>
        public static string ReverseIp(this string ip)
        {
            string[] ip_parts = ip.Split('.');
            Array.Reverse(ip_parts);
            string reversedIpAddress = string.Join(".", ip_parts);
            return reversedIpAddress;
        }


        /// <summary>
        /// The function converts a mac address to byte array
        /// </summary>
        /// <param name="macAddress">Mac address to convert</param>
        /// <returns>byte array representing the mac address</returns>
        public static byte[] ToBytes(this MacAddress macAddress)
        {
            byte[] byted = BitConverter.GetBytes(macAddress.ToValue()); // convert 6 bytes to 8 (BitConverter.GetBytes(long value))

            Array.Resize(ref byted, byted.Length - 2);  // last 2 bytes is zeros (because only 6 bytes used), to avoid issues, we will remove them.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byted);

            return byted;
        }


        /// <summary>
        /// The function converts an ip address to byte array
        /// </summary>
        /// <param name="ipV4Address">Ip address to convert</param>
        /// <returns>byte array representing the ip address</returns>
        public static byte[] ToBytes(this IpV4Address ipV4Address)
        {
            byte[] byted = BitConverter.GetBytes(ipV4Address.ToValue());

            if (BitConverter.IsLittleEndian)
                Array.Reverse(byted);

            return byted;
        }


        /// <summary>
        /// The function checks if it's a valid arp packet
        /// </summary>
        /// <param name="packet">Packet to check</param>
        /// <returns>True if valid, false if not</returns>
        public static bool IsValidARP(this Packet packet)
        {
            return packet.Ethernet.Arp != null && packet.Ethernet.Arp.IsValid && packet.Ethernet.Arp.TargetProtocolIpV4Address != null;
        }


        /// <summary>
        /// The function checks if it's a valid udp packet
        /// </summary>
        /// <param name="packet">Packet to check</param>
        /// <returns>True if valid, false if not</returns>
        public static bool IsValidUDP(this Packet packet, ushort destPort, ushort sourcePort = 0)
        {
            return sourcePort == 0
                ? packet.Ethernet.IpV4.Udp != null && packet.Ethernet.IpV4.Udp.DestinationPort == destPort
                : packet.Ethernet.IpV4.Udp != null && packet.Ethernet.IpV4.Udp.SourcePort == sourcePort && packet.Ethernet.IpV4.Udp.DestinationPort == destPort;
        }
    }
}
