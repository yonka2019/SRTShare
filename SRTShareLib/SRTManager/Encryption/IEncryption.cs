namespace SRTShareLib.SRTManager.Encryption
{
    public interface IEncryption
    {
        EncryptionType Type { get; }
        byte[] Encrypt(string plainText, byte[] Key, byte[] IV);
        byte[] Decrypt(byte[] cipherText, byte[] Key, byte[] IV);

    }
}
