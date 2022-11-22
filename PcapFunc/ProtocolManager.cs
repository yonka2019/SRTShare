﻿using PcapDotNet.Packets;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SRTManager
{
    public class ProtocolManager
    {
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
            public static void Induction(string ip, ushort port, bool first)
            {
                DateTime now  = DateTime.Now;
                string cookie = GenCookie(ip, port, now);

                ProtocolFields.Handshake handshakePacket;

                // change peer id to real one
                if(first)
                {
                    handshakePacket = new ProtocolFields.Handshake(4, 0, 2, 0, 1500, 1000, (uint)ProtocolFields.Handshake.HandshakeType.INDUCTION, 0, cookie, 0);
                }

                else
                {
                    handshakePacket = new ProtocolFields.Handshake(4, 0, 2, 0, 1500, 1000, (uint)ProtocolFields.Handshake.HandshakeType.INDUCTION, 0, "0", 0);
                }
            }

        }
    }
}