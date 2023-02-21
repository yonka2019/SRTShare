using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption  // Key Exchange Manager
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;

        private static byte[] secretKey;
        public static byte[] MyPublicKey { get; private set; }
        public static byte[] PeerPublicKey { private get; set; }

        public const int KEY_SIZE = 32;  // bytes

        static DiffieHellman()
        {
            me = new ECDiffieHellmanCng()  // 256 bit -> 32 bytes key fixed-size
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.MD5
            };
            me.KeySize = KEY_SIZE * 8;  // byte to bit // TODO: SHOULD BE FIXED TO BE 32 BYTES INSTAED OF BUGGED 72
            
            MyPublicKey = GetPublicKey();
            secretKey = null;  // should be set later, when second public key will be received
        }

        private static byte[] GetPublicKey()
        {
            return me.PublicKey.ToByteArray();
        }

        public static byte[] GetSecretKey()
        {
            if (secretKey == null)  // secret key doesn't exist - he should be created
            {
                if (PeerPublicKey == null)
                    throw new System.Exception("[ERROR] There is no public key to create secret key");

                else
                {
                    CngKey peerCngKey = CngKey.Import(PeerPublicKey, CngKeyBlobFormat.EccPublicBlob);
                    secretKey = me.DeriveKeyMaterial(peerCngKey);
                }
            }
            return secretKey;
        }
    }
}
