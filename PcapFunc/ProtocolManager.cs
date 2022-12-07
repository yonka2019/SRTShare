using System;
using System.Security.Cryptography;
using System.Text;

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

        public static uint GenerateSocketId(string ip, ushort port)
        {
            string textToEncrypt = "";
            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port

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


    }
}