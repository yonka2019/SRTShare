using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;

namespace SRTLibrary
{
    public static class MethodExt
    {
        public static string ReverseIp(this string input)
        {
            string[] ip_parts = input.Split('.');
            Array.Reverse(ip_parts);
            string reversedIpAddress = string.Join(".", ip_parts);
            return reversedIpAddress;
        }

        public static byte[] ToBytes(this MacAddress macAddress)
        {
            byte[] byted = BitConverter.GetBytes(macAddress.ToValue()); // convert 6 bytes to 8 (BitConverter.GetBytes(long value))

            Array.Resize(ref byted, byted.Length - 2);  // last 2 bytes is zeros (because only 6 bytes used), to avoid issues, we will remove them.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byted);

            return byted;
        }

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
        public static bool IsArp(this Packet packet)
        {
            return packet.Ethernet.Arp != null && packet.Ethernet.Arp.IsValid && packet.Ethernet.Arp.TargetProtocolIpV4Address != null;
        }
    }
}
