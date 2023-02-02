using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

namespace SRTShareLib.SRTManager.Encryption
{
    /* 
    * Each encryption method (type) must have his own 'public static class'
    * This class MUST have atleast the next functions:
    * - internal byte[] Encrypt(..)
    * - internal byte[] Decrypt(..)
    * - public byte[] CreateKey(..)
    * + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 
    * In addition, you must update the next files to support the new encryption:
    * - EncryptionManager.cs (this) ; update the conditions in order to use the suitable enc/dec
    * - EncryptionType.cs ; add the new type as supported
    * - MainView.cs ; update the conditions in order to get the suitable key (+IV?) for decryption
    */
    public class EncryptionManager
    {
        /// <summary>
        /// Encrypt the given data
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(EncryptionType encryptionType, byte[] data, ILayer[] layers)
        {
            IpV4Layer ipLayer = (IpV4Layer)layers[1];
            UdpLayer udpLayer = (UdpLayer)layers[2];

            string dstIp = ipLayer.Destination.ToString();
            ushort dstPort = ushort.Parse(udpLayer.DestinationPort.ToString());

            switch (encryptionType)
            {
                case EncryptionType.AES128:
                    {
                        byte[] key = AES128.CreateKey(dstIp, dstPort);
                        byte[] IV = AES128.CreateIV(ProtocolManager.GenerateSocketId(dstIp, dstPort).ToString());

                        return AES128.Encrypt(data, key, IV);
                    }

                case EncryptionType.Sub:
                    {
                        byte[] key = Substitution.CreateKey(dstIp, dstPort);

                        return Substitution.Encrypt(data, key);
                    }

                case EncryptionType.XOR:
                    {
                        byte[] key = XOR.CreateKey(dstIp, dstPort);

                        return XOR.Encrypt(data, key);
                    }

                default:
                    throw new System.Exception($"'{encryptionType}' This encryption method isn't supported yet");
            }
        }

        /// <summary>
        /// Decrypt the given data
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="key">key to decrypt with (according the selected encryption)</param>
        /// <param name="IV">iv to decrypt with (chosen AES-XXX method)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] Decrypt(EncryptionType encryptionType, byte[] data, byte[] key, byte[] IV = null)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES128:
                    return AES128.Decrypt(data, key, IV);

                case EncryptionType.Sub:
                    return Substitution.Decrypt(data, key);

                default:
                    throw new System.Exception($"'{encryptionType}' This decryption method isn't supported yet");
            }
        }

        /// <summary>
        /// Tries decrypt the given data, if the data can't be decrypted, thats mean that it is not the data, so return the data without any changes.
        /// </summary>
        /// <param name="data">data to decrypt</param>
        /// <param name="Key">key, which is 'DST_IP:DST_PORT' hashed into MD5 (128 bit)</param>
        /// <param name="IV">initialization vector, which is the CLIENT_SOCKET_ID hashed into MD5 (128 bit)</param>
        /// <param name="encryptionType">encrpytion type</param>
        /// <returns>Decrypted data</returns>
        public static byte[] TryDecrypt(EncryptionType encryptionType, byte[] data, byte[] Key, byte[] IV = null)
        {
            try
            {
                return Decrypt(encryptionType, data, Key, IV);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[CRYPTO] ERROR: Bad decryption ({e.Message})\n");  // cryptography issue
                return data;
            }
        }
    }
}
