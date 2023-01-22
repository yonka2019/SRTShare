using SRTShareLib.SRTManager.Encryption;
using System.IO;
using System.Security.Cryptography;

namespace SRTShareLib.SRTManager
{
    internal static class AES128
    {
        internal static EncryptionType Type => EncryptionType.AES128;

        internal static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return encrypted;
        }

        internal static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] decrypted;

            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(data))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream outputStream = new MemoryStream())
                        {
                            cs.CopyTo(outputStream);
                            decrypted = outputStream.ToArray();
                        }
                    }
                }
            }
            // Return decrypted data
            return decrypted;
        }
    }
}
