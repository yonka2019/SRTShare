using System;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class EncryptionFactory
    {
        public static BaseEncryption CreateEncryption(EncryptionType encryptionType, byte[] peerPublicKey)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES256:
                    return new AES256(peerPublicKey);

                case EncryptionType.Substitution:
                    return new Substitution(peerPublicKey);

                case EncryptionType.XOR:
                    return new XOR(peerPublicKey);

                case EncryptionType.None:
                    return new None(peerPublicKey);

                default:
                    throw new Exception($"'{encryptionType}' This encryption method isn't supported yet");
            }
        }
    }
}
