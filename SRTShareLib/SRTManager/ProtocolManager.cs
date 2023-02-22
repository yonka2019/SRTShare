using System;
using System.Security.Cryptography;
using System.Text;

namespace SRTShareLib
{
    public class ProtocolManager
    {
        public const long DEFAULT_QUALITY = 50L;  // video default quality value

        /// <summary>
        /// The function generates a syn cookie for the handshake
        /// </summary>
        /// <param name="ip">Client's ip</param>
        /// <param name="port">Client's port</param>
        /// <param name="current_time">Current time</param>
        /// <returns></returns>
        public static uint GenerateCookie(string ip)
        {
            string textToEncrypt = "";
            textToEncrypt += ip + "&cookie";  // add ip

            return Md5Encrypt(textToEncrypt);  // return the encrypted socket id
        }

        /// <summary>
        /// The function generates a socket id for the handshake
        /// </summary>
        /// <param name="ip">Client's ip</param>
        /// <param name="port">Client's port</param>
        /// <returns></returns>
        public static uint GenerateSocketId(string ip)
        {
            string textToEncrypt = "";
            textToEncrypt += ip + "&socket_id";  // add ip

            return Md5Encrypt(textToEncrypt);  // return the encrypted socket id
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
