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

        static byte[] substitutionTable = subTable();

        public static byte[] subTable()
        {
            byte[] table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                table[i] = (byte)((i + 128) % 256);
            }

            return table;
        }

        internal static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] encrypted = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = substitutionTable[data[i]];
            }
            return encrypted;
        }

        internal static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV)
        {
            byte[] decrypted = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < substitutionTable.Length; j++)
                {
                    if (data[i] == substitutionTable[j])
                    {
                        decrypted[i] = (byte)j;
                        break;
                    }
                }
            }
            return decrypted;
        }


        //internal static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        //{
        //    byte[] encrypted;

        //    using (AesManaged aes = new AesManaged())
        //    {
        //        aes.Padding = PaddingMode.Zeros;

        //        ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);

        //        using (MemoryStream ms = new MemoryStream())
        //        {   
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        //            {
        //                cs.Write(data, 0, data.Length);
        //            }
        //            encrypted = ms.ToArray();
        //        }
        //    }
        //    return encrypted;
        //}

        //internal static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV)
        //{
        //    byte[] decrypted;

        //    using (AesManaged aes = new AesManaged())
        //    {
        //        aes.Padding = PaddingMode.Zeros;

        //        ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);

        //        using (MemoryStream ms = new MemoryStream(data))
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
        //            {
        //                cs.Write(data, 0, data.Length);
        //            }
        //            decrypted = ms.ToArray();
        //        }
        //    }
        //    return decrypted;
        //}
    }
}
