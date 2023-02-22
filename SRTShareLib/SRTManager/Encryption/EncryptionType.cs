namespace SRTShareLib.SRTManager.Encryption
{
    public enum EncryptionType
    {
        None = 0,
        // AES128 = 2,  NOT SUPPORTED
        // AES192 = 3,  NOT SUPPORTED
        AES256 = 4,
        Substitution = 5,
        XOR = 6
    }
}
