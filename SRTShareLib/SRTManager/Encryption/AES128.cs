using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class AES128
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        internal static EncryptionType Type => EncryptionType.AES128;

        internal static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            using (AesManaged aes = new AesManaged())
            {
                aes.Padding = PaddingMode.Zeros;

                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    encrypted = ms.ToArray();
                }
            }
            return encrypted;
        }

        internal static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] decrypted;

            using (AesManaged aes = new AesManaged())
            {
                aes.Padding = PaddingMode.Zeros;

                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);

                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    decrypted = ms.ToArray();
                }
            }
            return decrypted;
        }

        /// <summary>
        /// According the encryption policy, the encryption key generates according the 'IP:PORT' Encrypted into hashed size (according the encryption type)
        /// AES128 - Key is hashed into MD5 (128 bit)
        /// </summary>
        /// <returns>ready hashed key to be used for encryption or decryption</returns>
        public static byte[] CreateKey(string ip, ushort port)
        {
            string keyToHash = $"{ip}:{port}";
            byte[] key;

            using (MD5 md5 = MD5.Create())
            {
                key = md5.ComputeHash(Encoding.UTF8.GetBytes(keyToHash));
            }
            return key;
        }

        /// <summary>
        /// According the encryption policy, the IV generates according the 'CLIENT_SOCKET_ID' field which is encrypted into hashed size 16 byte (128 bit) via MD5
        /// </summary>
        /// <returns>ready hashed iv to be used for encryption or decryption</returns>
        public static byte[] CreateIV(string socketId)
        {
            byte[] IV;

            using (MD5 md5 = MD5.Create())
            {
                IV = md5.ComputeHash(Encoding.UTF8.GetBytes(socketId));
            }
            return IV;
        }
    }
}
