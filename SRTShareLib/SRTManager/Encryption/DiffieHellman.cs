using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption  // Key Exchange Manager
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;

        private static byte[] secretKey;
        public static byte[] PublicKey { get; private set; }

        static DiffieHellman()
        {
            me = new ECDiffieHellmanCng(ECCurve.CreateFromFriendlyName("ECDH_P256"))  // 256 bit -> 32 bytes key fixed-size
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.MD5
            };
            PublicKey = GetPublicKey();
            secretKey = null;  // should be set later, when second public key will be received
        }

        public static byte[] GetPublicKey()
        {
            return me.PublicKey.ToByteArray();
        }

        public static byte[] GetSecretKey(byte[] peerPublicKey = null)
        {
            if (peerPublicKey == null)
            {
                return secretKey;
            }
            else  // secret key should be updated according the given peer public key
            {
                CngKey peerCngKey = CngKey.Import(peerPublicKey, CngKeyBlobFormat.EccPublicBlob);
                secretKey = me.DeriveKeyMaterial(peerCngKey);

                return secretKey;
            }
        }
    }
}
