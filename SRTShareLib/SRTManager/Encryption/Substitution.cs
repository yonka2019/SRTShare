namespace SRTShareLib.SRTManager.Encryption
{
    internal static class Substitution
    {
        internal static EncryptionType Type => EncryptionType.Sub128;

        private static byte[] substitutionTable;

        static Substitution()
        {
            substitutionTable = BuildTable();
        }

        /// <summary>
        /// Build substitution rules table
        /// </summary>
        /// <returns></returns>
        private static byte[] BuildTable()
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
    }
}