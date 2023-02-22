namespace SRTShareLib.SRTManager.Encryption
{
    /* 
    * Each encryption method (type) must have his own 'public static class' in "\EncryptionAlgorithms" folder (don't change the namespace)
    * THIS class MUST have atleast the next functions:
    * - internal byte[] Encrypt(..)
    * - internal byte[] Decrypt(..)
    * - public byte[] CreateKey(..)
    * + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 
    * In addition, you must update the next files to support the new encryption:
    * - EncryptionManager.cs [Encrypt, Decrypt] (this) ; update the conditions in order to use the suitable enc/dec
    * - EncryptionType.cs [EncryptionType] ; add the new type as supported
    * - MainView.cs [DecryptionNecessity]; update the conditions in order to get the suitable key (+IV?) for decryption
    */
    public class EncryptionManager
    {
        /// <summary>
        /// Encrypt the given data
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(byte[] data, EncryptionType encryptionType, byte[] key)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES256:
                    return AES256.Encrypt(data, key);

                case EncryptionType.Substitution:
                    return Substitution.Encrypt(data, key);

                case EncryptionType.XOR:
                    return XOR.Encrypt(data, key);

                default:
                    throw new System.Exception($"'{encryptionType}' This encryption method isn't supported yet");
            }
        }

        /// <summary>
        /// Decrypt the given data
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="key">key to decrypt with (according the selected encryption)</param>
        /// <param name="IV">iv to decrypt with (chosen AES-XXX method)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(EncryptionType encryptionType, byte[] data, byte[] key)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES256:
                    return AES256.Decrypt(data, key);

                case EncryptionType.Substitution:
                    return Substitution.Decrypt(data, key);

                case EncryptionType.XOR:
                    return XOR.Decrypt(data, key);

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
        public static byte[] TryDecrypt(EncryptionType encryptionType, byte[] data, byte[] Key)
        {
            try
            {
                return Decrypt(encryptionType, data, Key);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[CRYPTO] ERROR: Bad decryption ({e.Message})\n");  // cryptography issue
                return data;
            }
        }
    }
}
