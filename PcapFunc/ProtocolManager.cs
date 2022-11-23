using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using F_Handshake = SRTManager.ProtocolFields.Handshake;

namespace SRTManager
{
    public class ProtocolManager
    {
        public static List<IPAddress> sockets = new List<IPAddress>();

        /* Usage Example:
            EthernetLayer ethernetLayer = PacketManager.BuildEthernetLayer();
            IpV4Layer ipV4Layer = PacketManager.BuildIpv4Layer();
            UdpLayer udpLayer = PacketManager.BuildUdpLayer(PacketManager.SERVER_PORT, 123);
            var a = new ProtocolManager(ethernetLayer, ipV4Layer, udpLayer);
            Packet doneToSend = a.HandshakeRequest();
         */

        private readonly ILayer[] workingLayers; // layers we working with, we should add last packet data (SRT)
        /*
         * EthernetLayer - Exist
         * InternetLayer - Exist
         * UDPLayer - Exist
         * PayloadLayer (SRT Data) - Should be added
         */

        public ProtocolManager(params ILayer[] layers)
        {
            workingLayers = new ILayer[layers.Length + 1]; // add payload layer after
            layers.CopyTo(workingLayers, 0);
        }

        public Packet HandshakeRequest(string ip, ushort port, DateTime current_time) // add data to working packet
        {
            PayloadLayer pLayer = PacketManager.BuildPLayer("lala");
            workingLayers[workingLayers.Length - 1] = pLayer;

            string my_cookie = GenCookie(ip, port, current_time); // get cookie

            Console.WriteLine(my_cookie);

            return new PacketBuilder(workingLayers).Build(DateTime.Now);
        }

        private static string GenCookie(string ip, ushort port, DateTime current_time)
        {
            string textToEncrypt = "";

            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port
            textToEncrypt += $"{current_time.Second}.{current_time.Minute}.{current_time.Hour}.{current_time.Day}.{current_time.Month}.{current_time.Year}"; // add current time

            return Md5Encrypt(textToEncrypt); // return the encrypted cookie
        }

        private static string Md5Encrypt(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            string strBuilder = "";
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder += result[i].ToString("x2");
            }

            return strBuilder;
        }

        public class Handshake
        {
            // public Handshake(uint version, ushort encryption_field, ushort extension_field, uint intial_psn,
            // uint mtu, uint mfws, uint type, uint socket_id, string syn_cookie, decimal p_ip)
            public static void Induction(string ip, ushort port, bool clientSide)
            {
                DateTime now = DateTime.Now;
                string cookie = GenCookie(ip, port, now);

                F_Handshake handshakePacket;

                if (clientSide)
                {
                    // CALLER -> LISTENER (first message)
                    handshakePacket = new F_Handshake(version: 4, 0, 0, 1000, 1000, (uint)F_Handshake.HandshakeType.INDUCTION, 0, cookie, 0);
                }

                else
                { // LISTENER -> CALLER (first message RESPONSE)
                    handshakePacket = new F_Handshake(version: 5, 0, 0, 1000, 1000, (uint)F_Handshake.HandshakeType.INDUCTION, (uint)sockets.Count, "0", 0);
                }
            }

        }
    }
}