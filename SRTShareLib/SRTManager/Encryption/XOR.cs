using System.Text;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class XOR
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        public const EncryptionType Type = EncryptionType.Substitution;
        // dynamic key size, according the: 'ip' + 'port'

        internal static byte[] Encrypt(byte[] data, byte[] key)
        {
            return Cipher(data, key);
        }

        internal static byte[] Decrypt(byte[] data, byte[] key)
        {
            return Cipher(data, key);  // encrypt to encrypt = decrypted (especially for xor encryption)
        }

        private static byte[] Cipher(byte[] data, byte[] key)
        {
            int dataLen = data.Length;
            int keyLen = key.Length;
            byte[] output = new byte[dataLen];

            for (int i = 0; i < dataLen; ++i)
            {
                output[i] = (byte)(data[i] ^ key[i % keyLen]);
            }

            return output;
        }

        public static (byte[], byte[]) CreateKey(string ip)
        {
            return (Encoding.ASCII.GetBytes(ip), null);  // null - is the IV (not in using)
        }
    }
}
