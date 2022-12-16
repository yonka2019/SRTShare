using System;
using System.Security.Cryptography;
using System.Text;

namespace CLib
{
    public class ProtocolManager
    {
        /// <summary>
        /// The function generates a syn cookie for the handshake
        /// </summary>
        /// <param name="ip">Client's ip</param>
        /// <param name="port">Client's port</param>
        /// <param name="current_time">Current time</param>
        /// <returns></returns>
        public static uint GenerateCookie(string ip, ushort port, DateTime current_time)
        {
            string textToEncrypt = "";

            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port
            textToEncrypt += $"{current_time.Minute}.{current_time.Hour}.{current_time.Day}.{current_time.Month}.{current_time.Year}"; // add current time

            return Md5Encrypt(textToEncrypt); // return the encrypted cookie
        }

        /// <summary>
        /// The function gtenerates a socket id for the handshake
        /// </summary>
        /// <param name="ip">Client's ip</param>
        /// <param name="port">Client's port</param>
        /// <returns></returns>
        public static uint GenerateSocketId(string ip, ushort port)
        {
            string textToEncrypt = "";
            textToEncrypt += ip + "&"; // add ip
            textToEncrypt += port.ToString() + "&"; // add port

            return Md5Encrypt(textToEncrypt); // return the encrypted cookie
        }

        /// <summary>
        /// The function encrypts the given text
        /// </summary>
        /// <param name="text">Text to encrypt</param>
        /// <returns>Encrypted text in bytes</returns>
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
}