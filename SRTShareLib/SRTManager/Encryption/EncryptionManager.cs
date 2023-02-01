namespace SRTShareLib.SRTManager.Encryption
{
    public class EncryptionManager
    {
        /// <summary>
        /// Encrypt the given data
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="Key">key, which is 'DST_IP:DST_PORT' hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the (DST)CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES128:
                    return AES128.Encrypt(data, Key, IV);

                default:
                    throw new System.Exception($"'{encryptionType}' This encryption method isn't supported yet");
            }
        }

        /// <summary>
        /// Decrypt the given data
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="Key">key, which is 'DST_IP:DST_PORT' hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES128:
                    return AES128.Decrypt(data, Key, IV);
                case EncryptionType.Sub128:
                    return Substitution.

                default:
                    throw new System.Exception($"'{encryptionType}' This decryption method isn't supported yet");
            }
        }

        /// <summary>
        /// Tries decrypt the given data, if the data can't be decrypted, thats mean that it is not the data, so return the data without any changes.
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="Key">key, which is 'DST_IP:DST_PORT' hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] TryDecrypt(EncryptionType encryptionType, byte[] data, byte[] Key, byte[] IV = null)
        {
            try
            {
                return Decrypt(data, Key, IV, encryptionType);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[CRYPTO] ERROR: Bad decryption ({e.Message})\n");  // cryptography issue
                return data;
            }
        }
    }
}
