using System;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class Substitution
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        public const EncryptionType Type = EncryptionType.Substitution;

        /// <summary>
        /// Build substitution rules table
        /// </summary>
        /// <returns>Ready substituted table</returns>
        private static byte[] BuildTable(byte[] _key)
        {
            int key = BitConverter.ToInt32(_key, 0);

            byte[] table = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                table[i] = (byte)((i + key) % 256);
            }
            return table;
        }

        internal static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] subTable = BuildTable(key);

            byte[] encrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = subTable[data[i]];
            }
            return encrypted;
        }

        internal static byte[] Decrypt(byte[] data, byte[] key)
        {
            byte[] subTable = BuildTable(key);

            byte[] decrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < subTable.Length; j++)
                {
                    if (data[i] == subTable[j])
                    {
                        decrypted[i] = (byte)j;
                        break;
                    }
                }
            }
            return decrypted;
        }
    }
}