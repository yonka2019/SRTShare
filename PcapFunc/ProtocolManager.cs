using PcapDotNet.Packets;
using System;
using System.Security.Cryptography;
using System.Text;

using F_Handshake = SRTManager.ProtocolFields.Handshake;

namespace SRTManager
{
    public class ProtocolManager
    {
        public static uint GenerateCookie(string ip, ushort port, DateTime current_time)
        {
            string textToEncrypt = "";

            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port
            textToEncrypt += $"{current_time.Minute}.{current_time.Hour}.{current_time.Day}.{current_time.Month}.{current_time.Year}"; // add current time

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
            public Packet Induction(uint cookie, uint init_psn, double p_ip, bool clientSide, int socket_id = 0)
            {
               
                F_Handshake f_handshake;

                if (clientSide)
                {
                    // CALLER -> LISTENER (first message REQUEST) [CLIENT -> SERVER]
                    f_handshake = new F_Handshake(version: 4, 0, init_psn, (uint)F_Handshake.HandshakeType.INDUCTION, (uint)socket_id, 0, p_ip);
                }

                else
                {
                    // LISTENER -> CALLER (first message RESPONSE) [SERVER -> CLIENT]
                    f_handshake = new F_Handshake(version: 5, 0, init_psn, (uint)F_Handshake.HandshakeType.INDUCTION, (uint)socket_id, cookie, p_ip);
                }

                GetPayloadLayer() = PacketManager.BuildPLayer(f_handshake.GetByted()); // set last payload layer as our srt packet

                return BuildPacket();
            }


            public Packet Conclusion(uint init_psn, double p_ip, bool clientSide, int socket_id = 0, uint cookie = 0)
            {
                F_Handshake f_handshake;

                if (clientSide)
                {
                    // CALLER -> LISTENER (second message REQUEST) [CLIENT -> SERVER]
                    f_handshake = new F_Handshake(version: 5, 0, init_psn, (uint)F_Handshake.HandshakeType.CONCLUSION, (uint)socket_id, cookie, p_ip);
                }

                else
                {
                    // LISTENER -> CALLER (second message RESPONSE) [SERVER -> CLIENT]
                    f_handshake = new F_Handshake(version: 5, 0, init_psn, (uint)F_Handshake.HandshakeType.CONCLUSION, (uint)socket_id, 0, p_ip);
                }

                GetPayloadLayer() = PacketManager.BuildPLayer(f_handshake.GetByted()); // set last payload layer as our srt packet

                return BuildPacket();
            }

        }
    }
}