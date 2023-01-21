namespace SRTShareLib.SRTManager.Encryption
{
    public interface IEncryption
    {
        /// <summary>
        /// Type of the encryption
        /// </summary>
        EncryptionType Type { get; }

        /// <summary>
        /// Encrypt the given data
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="Key">key, which is IP&PORT hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <returns>encrypted data</returns>
        byte[] Encrypt(byte[] data, byte[] Key, byte[] IV);

        /// <summary>
        /// Decrypt the given data
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="Key">key, which is IP&PORT hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <returns>decrypted data</returns>
        byte[] Decrypt(byte[] data, byte[] Key, byte[] IV);
    }
}
