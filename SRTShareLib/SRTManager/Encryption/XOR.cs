using System.Linq;
using System.Text;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class XOR
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        internal static EncryptionType Type => EncryptionType.Sub;

        internal static byte[] Encrypt(byte[] data, byte[] key)
        {
            return Cipher(data, key);
        }

        internal static byte[] Decrypt(byte[] data, byte[] key)
        {
            return  Cipher(data, key);
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

        public static byte[] CreateKey(string ip, ushort port)
        {
            return Encoding.ASCII.GetBytes(string.Join(ip, port.ToString()));
        }

    }
}
