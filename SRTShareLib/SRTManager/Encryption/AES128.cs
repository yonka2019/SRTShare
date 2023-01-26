using SRTShareLib.SRTManager.Encryption;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;

namespace SRTShareLib.SRTManager
{
    internal static class AES128
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
    }
}
