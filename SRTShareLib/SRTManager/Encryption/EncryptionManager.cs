using System.Security.Cryptography;
using System.Text;

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

                default:
                    return null;
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
        public static byte[] TryDecrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryptionType)
        {
            try
            {
                return Decrypt(data, Key, IV, encryptionType);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] Bad decryption");
                return data;
            }
        }

        /// <summary>
        /// According the encryption policy, the encryption key generates according the 'IP:PORT' Encrypted into hashed size (according the encryption type)
        /// AES128 - Key is hashed into MD5 (128 bit)
        /// AES256 - Key is hashed into SHA256 (256 bit)
        /// . . .
        /// </summary>
        /// <returns>ready hashed key to be used for encryption or decryption</returns>
        public static byte[] CreateKey(string ip, ushort port, EncryptionType encryptionType)
        {
            if (encryptionType == EncryptionType.AES128)
            {
                string keyToHash = $"{ip}:{port}";
                byte[] key;

                using (MD5 md5 = MD5.Create())
                {
                    key = md5.ComputeHash(Encoding.UTF8.GetBytes(keyToHash));
                }
                return key;
            }
            return null;
        }

        /// <summary>
        /// According the encryption policy, the IV generates accoridng the 'CLIENT_SOCKET_ID' field which is encrypted into hashed size 16 byte (128 bit) via MD5
        /// </summary>
        /// <returns>ready hashed iv to be used for encryption or decryption</returns>
        public static byte[] CreateIV(string socketId)
        {
            byte[] IV;

            using (MD5 md5 = MD5.Create())
            {
                IV = md5.ComputeHash(Encoding.UTF8.GetBytes(socketId));
            }
            return IV;
        }
    }
}
