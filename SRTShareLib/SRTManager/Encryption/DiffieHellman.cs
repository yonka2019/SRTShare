using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption  // Key Exchange Manager
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;

        private static byte[] secretKey;
        public static byte[] MyPublicKey { get; private set; }
        public static byte[] PeerPublicKey { private get; set; }

        /// <summary>
        /// on chaning this public key size, the following classes should be updated as well:
        /// SRTManagaer.ProtocolFields.Control.Handshake
        /// </summary>
        public const int PUBLIC_KEY_SIZE = 72;  // bytes

        static DiffieHellman()
        {
            // brainpool causes to "Invalid paramater" On CngKey.Import, so the best one - nistP256 Curve
            me = new ECDiffieHellmanCng(ECCurve.NamedCurves.nistP256);  // 256 bit -> 32 bytes secret key fixed-size

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
