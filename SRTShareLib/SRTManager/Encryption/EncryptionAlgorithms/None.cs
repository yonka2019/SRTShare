namespace SRTShareLib.SRTManager.Encryption
{
    internal class None : BaseEncryption
    {
        internal None(byte[] peerPublicKey) : base(EncryptionType.None, peerPublicKey) { }

        internal override byte[] Decrypt(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        internal override byte[] Encrypt(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}