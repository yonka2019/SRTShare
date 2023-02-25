using System;
using System.IO;
using System.Security.Cryptography;

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
     * AES256 - depend on the other bytes because the hash encoding which is problematically in UDP based protocol (although, retransmission could help)
     * Substitution / XOR - doesn't depend on other bytes, so they have better performance
     */
    public static class AES256
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        public const EncryptionType Type = EncryptionType.AES256;
        public const int IVSize = 16;  // Bytes. Default to all AES encryption types (AES128, AES192, AES256 ..)

        internal static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] encrypted;

            byte[] IV = new byte[IVSize];
            Array.Copy(key, 0, IV, 0, 16);

            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(key, IV);

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

        internal static byte[] Decrypt(byte[] data, byte[] key)
        {
            byte[] decrypted;

            byte[] IV = new byte[IVSize];
            Array.Copy(key, 0, IV, 0, 16);

            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, IV);

                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream msDecrypt = new MemoryStream())
                        {
                            byte[] buffer = new byte[1024];
                            int read;

                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypt.Write(buffer, 0, read);
                            }
                            decrypted = msDecrypt.ToArray();
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
