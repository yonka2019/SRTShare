using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SRTShareLib.SRTManager.Encryption
{
    /*
     * This encryption method is problematic because this encryption method use "hash" function in order to encrypt/decrypt the data.
     * Which means, that even if one bit will lost from the packet or will received in the wrong way, the whole packet will be damaged and unsuitable to decrypt.
     * In other words, this encrypytion method require the packet to be received fully as it was sent, and because the protocol which uses UDP connection as base,
     * the packets come damaged, and can't be decrypted and read correctly, which causes a lot of interference and unreadable screen-share.
     * There is two ways to cope with this issue, the first one is to add support of retransmission, to ensure that the packets would be received correctly.
     * Another way, is to use encryption which operates on each byte individually and doesn't depend on any other bytes in the list, with this way,
     * even if any bit lost, the packet will be avaible to decryption.
     * 
     * AES128 - depend on the other bytes because the hash encoding which is problematically in UDP based protocol (although, retransmission could help)
     * Substitution / XOR - doesn't depend on other bytes, so they have better performance
     */
    public static class AES128
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        public const EncryptionType Type = EncryptionType.AES128;
        public const int KeySize = 16;  // Bytes (AES 128 - 128 bit => 16 bit key size)
        public const int IVSize = 16;  // Default to all AES encryptions

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
        /// According the encryption policy, the IV generates according the 'CLIENT_SOCKET_ID' field which is encrypted into hashed size 16 byte (128 bit) via MD5
        /// </summary>
        /// <returns>ready hashed key to be used for encryption or decryption</returns>
        public static (byte[], byte[]) CreateKey_IV(string ip, ushort port)
        {
            byte[] key;
            byte[] IV;

            string socketId = ProtocolManager.GenerateSocketId(ip, port).ToString();
            string keyToHash = $"{ip}:{port}";

            using (MD5 md5 = MD5.Create())
            {
                key = md5.ComputeHash(Encoding.UTF8.GetBytes(keyToHash));
                IV = md5.ComputeHash(Encoding.UTF8.GetBytes(socketId));
            }

            return (key, IV);
        }
    }
}
