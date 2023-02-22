namespace SRTShareLib.SRTManager.Encryption
{
    /// <summary>
    /// This struct allows to create a instance of a "Peer Encryption information"
    /// SERVER EXAMPLE:
    /// Each client can choose his own encryption method, in addition, each client surely have his own public key, 
    /// this class helps to save the "conversation encryption data"
    /// CLIENT EXAMPLE:
    /// The server has his own public key. In order to save it and use it later, to decrypt the packets, this class helps to save the public key
    /// </summary>
    public readonly struct PeerEncryptionData
    {
        public readonly EncryptionType Type;

        public readonly byte[] PeerPublicKey;

        public readonly byte[] SecretKey;

        public PeerEncryptionData(EncryptionType encryptionType, byte[] peerPublicKey)
        {
            Type = encryptionType;
            PeerPublicKey = peerPublicKey;

            if (encryptionType == EncryptionType.None)  // if encryption type none - peer public key should be fulled zeros
                SecretKey = new byte[DiffieHellman.SECRET_KEY_SIZE];
            else
                SecretKey = DiffieHellman.GenerateSecretKey(peerPublicKey);

        }
    }
}
