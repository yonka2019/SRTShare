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
            int dataLen = data.Length;
            int keyLen = key.Length;
            char[] output = new char[dataLen];

            for (int i = 0; i < dataLen; ++i)
            {
                output[i] = (char)(data[i] ^ key[i % keyLen]);
            }

            return new string(output);
        }

    }
}
