using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption.KEManager  // Key Exchange Manager
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;

        public static byte[] PublicKey { get; private set; }

        static DiffieHellman()
        {
            me = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.MD5  // 128 bit key (16 byte)
            };

            PublicKey = GetPublicKey();
        }

        public static byte[] GetPublicKey()
        {
            return me.PublicKey.ToByteArray();
        }

        public static byte[] GenerateSecretKey(byte[] peerKey)
        {
            CngKey peerCngKey = CngKey.Import(peerKey, CngKeyBlobFormat.EccPublicBlob);
            return me.DeriveKeyMaterial(peerCngKey);
        }
    }
}
