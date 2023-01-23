namespace SRTShareLib.SRTManager.Encryption
{
    public class EncryptionManager
    {
        /// <summary>
        /// Encrypt the given data
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="Key">key, which is 'DST_IP|DST_PORT' hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the (DST)CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryption">encrpytion type</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryption)
        {
            switch (encryption)
            {
                case EncryptionType.AES128:
                    return AES128.Encrypt(data, Key, IV);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Decrypt the given data
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="Key">key, which is IP|PORT hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES128:
                    return AES128.Decrypt(data, Key, IV);

                default:
                    return null;
            }
        }
    }
}
