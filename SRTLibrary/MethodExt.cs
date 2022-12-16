using PcapDotNet.Base;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SRTLibrary
{
    public static class MethodExt
    {
        public static uint GetUInt32(this string str)
        {
            return BitConverter.ToUInt32(Encoding.ASCII.GetBytes(str), 0);
        }

        public static string Reverse(this string input)
        {
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
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
    }
}
