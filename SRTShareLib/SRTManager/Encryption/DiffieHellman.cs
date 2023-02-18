using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption  // Key Exchange Manager
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;

        public static byte[] PublicKey { get; private set; }

        static DiffieHellman()
        {
            me = new ECDiffieHellmanCng(ECCurve.CreateFromFriendlyName("ECDH_P256"))  // 256 bit -> 32 bytes key fixed-size
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.MD5
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
