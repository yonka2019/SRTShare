namespace SRTShareLib.SRTManager.Encryption
{
    /// <summary>
    /// This encryption method is problematic when using high quality (such as 100%..)
    /// It's took too much time to decrypt the packets - which causes to a HUGE delay between the server and the client
    /// </summary>
    internal class Substitution : BaseEncryption
    {
        internal Substitution(byte[] peerPublicKey) : base(EncryptionType.Substitution, peerPublicKey) { }

        /// <summary>
        /// Build substitution rules table
        /// </summary>
        /// <returns>Ready substituted table</returns>
        private byte[] BuildTable(byte[] _key)
        {
            byte key = _key[0];

            byte[] table = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                table[i] = (byte)((i + key) % 256);
            }
            return table;
        }

        internal override byte[] Encrypt(byte[] data)
        {
            byte[] subTable = BuildTable(key);

            byte[] encrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = subTable[data[i]];
            }
            return encrypted;
        }

        internal override byte[] Decrypt(byte[] data)
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