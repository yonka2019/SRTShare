using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SRTShareLib
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
            if (packet != null)
            {
                if (packet.Ethernet != null)
                {
                    if (packet.Ethernet.Arp != null)
                    {
                        if (packet.Ethernet.Arp.IsValid)
                        {
                            if (packet.Ethernet.Arp.TargetProtocolAddress.Count == 4 && packet.Ethernet.Arp.SenderProtocolAddress.Count == 4)
                            {
                                if (packet.Ethernet.Arp.TargetHardwareAddress.Count == 6 && packet.Ethernet.Arp.SenderHardwareAddress.Count == 6)
                                {
                                    if (packet.Ethernet.Arp.TargetProtocolIpV4Address != null && packet.Ethernet.Arp.SenderProtocolIpV4Address != null)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// The function checks if it's a valid udp packet
        /// </summary>
        /// <param name="packet">Packet to check</param>
        /// <returns>True if valid, false if not</returns>
        public static bool IsValidUDP(this Packet packet, ushort destPort, ushort sourcePort = 0)
        {
            if (packet != null)
            {
                if (packet.Ethernet != null)
                {
                    if (packet.Ethernet.IpV4 != null)
                    {
                        if (packet.Ethernet.IpV4.Udp != null)
                        {
                            if (packet.Ethernet.IpV4.Udp.DestinationPort == destPort && (sourcePort == 0 || packet.Ethernet.IpV4.Udp.SourcePort == sourcePort))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Convert byted mac to valid mac with (AA:BB:CC:DD:EE:FF)
        /// </summary>
        /// <param name="mac">byted mac collection to be converted</param>
        /// <returns>valid mac in the next format: 'AA:BB:CC:DD:EE:FF'</returns>
        public static string GetFormattedMac(ReadOnlyCollection<byte> mac)
        {
            return BitConverter.ToString(mac.ToArray()).Replace("-", ":");
        }

        /// <summary>
        /// Check if the given ip is the same subnet with the gateway
        /// </summary>
        /// <param name="ip">ip to check</param>
        /// <param name="subnet">subnet mask of the LAN</param>
        /// <param name="gateway">gateway of the active LAN interface</param>
        /// <returns>true if the ip is in the same subnet with the LAN gateway</returns>
        public static bool IsInSubnet(this IpV4Address ip, string subnet, string gateway)
        {
            int[] sIp = Array.ConvertAll(ip.ToString().Split('.'), i => int.Parse(i));
            int[] sSubnet = Array.ConvertAll(subnet.ToString().Split('.'), i => int.Parse(i));
            int[] sGateway = Array.ConvertAll(gateway.ToString().Split('.'), i => int.Parse(i));

            for (int i = 0; i < 4; i++)
            {
                if (sSubnet[i] == 255)
                {
                    if (sIp[i] != sGateway[i])
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Rounds the given number to ten, for example:
        /// 23 -> 30 | 29 -> 30 | 40 -> 40 | 41 -> 50
        /// </summary>
        /// <param name="num">number to round up to ten's</param>
        /// <returns>rounded number</returns>
        public static long RoundToNearestTen(this long num)
        {
            return num % 10 >= 5 ? (((num / 10) + 1) * 10) : (num / 10 * 10);
        }

        /// <summary>
        /// Calculating checksum via internet method [ORDER DEPENDED]
        /// </summary>
        /// <param name="data">data to calculate his checksum</param>
        /// <returns>data checksum</returns>
        public static ushort CalculateChecksum(this byte[] data)
        {
            uint sum = 0;
            int i = 0;

            // Sum all 16-bit words in the data
            while (i < data.Length - 1)
            {
                sum += (uint)((data[i] << 8) | data[i + 1]);
                i += 2;
            }

            // If the length of the data is odd, add the last byte as a padding byte
            if (i == data.Length - 1)
            {
                sum += (uint)(data[i] << 8);
            }

            // Fold the 32-bit sum to a 16-bit value
            while (sum >> 16 != 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            // Take the one's complement of the sum
            ushort checksum = (ushort)~sum;

            return checksum;
        }
    }
}
