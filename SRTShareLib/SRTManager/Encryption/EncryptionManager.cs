namespace SRTShareLib.SRTManager.Encryption
{
    public class EncryptionManager
    {
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

        public static byte[] Decrypt(byte[] data, byte[] Key, byte[] IV, EncryptionType encryption)
        {
            switch (encryption)
            {
                case EncryptionType.AES128:
                    return AES128.Decrypt(data, Key, IV);

                default:
                    return null;
            }
        }
    }
}
