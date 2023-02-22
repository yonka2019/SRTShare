using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public readonly struct PeerEncryption
    {
        public readonly EncryptionType Type;

        public readonly byte[] PeerPublicKey;

        public readonly byte[] SecretKey;

        public PeerEncryption(EncryptionType encryptionType, byte[] peerPublicKey)
        {
            Type = encryptionType;
            PeerPublicKey = peerPublicKey;
            SecretKey = DiffieHellman.GenerateSecretKey(peerPublicKey);
        }
    }
}
