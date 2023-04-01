namespace SRTShareLib.SRTManager.Encryption
{
    internal class None : BaseEncryption
    {
        internal None(byte[] peerPublicKey) : base(EncryptionType.None, peerPublicKey) { }

        public override byte[] Encrypt(byte[] data)
        {
            return data;
        }

        internal override byte[] Decrypt(byte[] data)
        {
            return data;
        }
    }
}