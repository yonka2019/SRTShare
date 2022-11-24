using PcapDotNet.Packets;
using System;
using System.Security.Cryptography;
using System.Text;

using F_Handshake = SRTManager.ProtocolFields.Handshake;

namespace SRTManager
{
    public class ProtocolManager
    {
        private static uint GenerateCookie(string ip, ushort port, DateTime current_time)
        {
            string textToEncrypt = "";

            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port
            textToEncrypt += $"{current_time.Second}.{current_time.Minute}.{current_time.Hour}.{current_time.Day}.{current_time.Month}.{current_time.Year}"; // add current time

            return Md5Encrypt(textToEncrypt); // return the encrypted cookie
        }

        private static uint Md5Encrypt(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(Encoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            return BitConverter.ToUInt32(md5.Hash, 0);
        }

        public class HandshakeRequest : UdpPacket
        {
            /*
             *  Usage Example:
                        var handshakeRequest = new SRTManager.ProtocolManager.Handshake(PacketManager.BuildEthernetLayer(),
                        PacketManager.BuildIpv4Layer(),
                        PacketManager.BuildUdpLayer(600, PacketManager.SERVER_PORT));
                        Packet readyToSent = a.Induction("a", 1, false);
            */

            public HandshakeRequest(params ILayer[] layers) : base(layers) { }

            // public F_Handshake(uint version, ushort encryption_field, uint intial_psn, uint type, uint socket_id, uint syn_cookie, decimal p_ip)
            public Packet Induction(string ip, ushort port, bool clientSide, int socket_id = 0)
            {
                DateTime now = DateTime.Now;
                uint cookie = GenerateCookie(ip, port, now);

                F_Handshake f_handshake;

                if (clientSide)
                {
                    // CALLER -> LISTENER (first message)
                    f_handshake = new F_Handshake(version: 4, 0, 0, (uint)F_Handshake.HandshakeType.INDUCTION, (uint)socket_id, 0, 0);
                }

                else
                {
                    // LISTENER -> CALLER (first message RESPONSE)
                    f_handshake = new F_Handshake(version: 5, 0, 0, (uint)F_Handshake.HandshakeType.INDUCTION, (uint)socket_id, cookie, 0);
                }

                GetPayloadLayer() = PacketManager.BuildPLayer(f_handshake.GetByted()); // set last payload layer as our srt packet

                return BuildPacket();
            }

            public static void Conclusion()
            {

            }

        }
    }
}