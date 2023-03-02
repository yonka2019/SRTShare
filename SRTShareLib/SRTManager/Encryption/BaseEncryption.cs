namespace SRTShareLib.SRTManager.Encryption
{
    public abstract class BaseEncryption
    {
        public EncryptionType Type { get; }

        protected readonly byte[] key;

        public BaseEncryption(EncryptionType encryptionType, byte[] peerPublicKey)
        {
            Type = encryptionType;

            if (encryptionType == EncryptionType.None)  // if encryption type none - peer public key should be fulled zeros
                key = new byte[DiffieHellman.SECRET_KEY_SIZE];
            else
                key = DiffieHellman.GenerateSecretKey(peerPublicKey);
        }

        internal abstract byte[] Encrypt(byte[] data);
        internal abstract byte[] Decrypt(byte[] data);

        public byte[] TryDecrypt(byte[] data)
        {
            try
            {
                return Decrypt(data);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[ENCRYPTION] ERROR: Decryption issue ({e.Message})\n");  // cryptography issue
                return data;
            }
        }
    }

    public enum EncryptionType
    {
        None = 0,
        // AES128 = 2,  NOT SUPPORTED
        // AES192 = 3,  NOT SUPPORTED
        AES256 = 4,
        Substitution = 5,
        XOR = 6
    }
}
