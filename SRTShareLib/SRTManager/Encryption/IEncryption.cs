namespace SRTShareLib.SRTManager.Encryption
{
    public interface IEncryption
    {
        EncryptionType Type { get; }
        byte[] Encrypt(byte[] data, byte[] Key, byte[] IV);
        byte[] Decrypt(byte[] data, byte[] Key, byte[] IV);
    }
}
