using System.Security.Cryptography;

namespace SRTShareLib.SRTManager.Encryption
{
    public static class DiffieHellman
    {
        private static readonly ECDiffieHellmanCng me;
        public static byte[] MyPublicKey { get; private set; }

        /// <summary>
        /// on chaning this public key size, the following classes should be updated as well:
        /// SRTManagaer.ProtocolFields.Control.Handshake
        /// </summary>
        public const int PUBLIC_KEY_SIZE = 72;  // bytes (576 bit total) ;;  via (ECCurve.NamedCurves.nistP256 (256bit) + parameters)
        public const int SECRET_KEY_SIZE = 32;  // bytes (256 bit total) ;;  via (ECCurve.NamedCurves.nistP256 (256bit)

        static DiffieHellman()
        {
            // brainpoolPXXX causes to "Invalid paramater" On CngKey.Import, so the best one - nistP256 Curve
            me = new ECDiffieHellmanCng(ECCurve.NamedCurves.nistP256);  // 256 bit -> 32 bytes SECRET key fixed-size

            MyPublicKey = GetPublicKey();
        }

        private static byte[] GetPublicKey()
        {
            return me.PublicKey.ToByteArray();
        }

        public static byte[] GenerateSecretKey(byte[] publicKey)
        {
            CngKey peerCngKey = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            return me.DeriveKeyMaterial(peerCngKey);
        }
    }
}
