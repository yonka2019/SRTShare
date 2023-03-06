namespace SRTShareLib.SRTManager.Encryption
{
    internal class XOR : BaseEncryption
    {
        internal XOR(byte[] peerPublicKey) : base(EncryptionType.XOR, peerPublicKey) { }

        public override byte[] Encrypt(byte[] data)
        {
            return Cipher(data, key);
        }

        internal override byte[] Decrypt(byte[] data)
        {
            return Cipher(data, key);  // encrypt to encrypt = decrypted (ESPECIALLY for XOR encryption)
        }

        private static byte[] Cipher(byte[] data, byte[] key)
        {
            byte[] output = new byte[data.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                output[i] = (byte)(data[i] ^ key[i % key.Length]);
            }

            return output;
        }
    }
}
