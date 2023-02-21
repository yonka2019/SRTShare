using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets.IpV4;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using System;

namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public class Handshake : SRTHeader
    {
        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public Handshake(uint version, ushort encryption_type, byte[] encryption_public_key, uint intial_psn, uint type, uint source_socket_id, uint dest_socket_id, uint syn_cookie, IpV4Address p_ip) : base(ControlType.HANDSHAKE, dest_socket_id)
        {
            VERSION = version; byteFields.Add(BitConverter.GetBytes(VERSION));

            ENCRYPTION_TYPE = encryption_type; byteFields.Add(BitConverter.GetBytes(ENCRYPTION_TYPE));
            ENCRYPTION_PUBLIC_KEY = encryption_public_key; byteFields.Add(ENCRYPTION_PUBLIC_KEY);

            INTIAL_PSN = intial_psn; byteFields.Add(BitConverter.GetBytes(INTIAL_PSN));

            // (.Mtu - 100; explanation) To avoid errors with sending, because this field used to set fixed size of splitted data packet, while the real mtu that the interface provides refers the whole size of the packet which get sent, and with the whole srt packet and all layers in will much more
            MTU = (uint)NetworkManager.Device.GetNetworkInterface().GetIPProperties().GetIPv4Properties().Mtu - 100; byteFields.Add(BitConverter.GetBytes(MTU));
            TYPE = type; byteFields.Add(BitConverter.GetBytes(TYPE));
            SOCKET_ID = source_socket_id; byteFields.Add(BitConverter.GetBytes(SOCKET_ID));
            SYN_COOKIE = syn_cookie; byteFields.Add(BitConverter.GetBytes(SYN_COOKIE));
            PEER_IP = p_ip; byteFields.Add(PEER_IP.ToBytes());
        }

        /// <summary>
        /// Byte[] -> Fields (To extract)
        /// </summary>
        public Handshake(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            VERSION = BitConverter.ToUInt32(data, 13);  // [13 14 15 16] (4 bytes)

            ENCRYPTION_TYPE = BitConverter.ToUInt16(data, 17);  // [17 18] (2 bytes)

            ENCRYPTION_PUBLIC_KEY = new byte[32];
            Array.Copy(data, 19, ENCRYPTION_PUBLIC_KEY, 0, DiffieHellman.KEY_SIZE);  // [19 ... 50] (32 bytes)
            
            INTIAL_PSN = BitConverter.ToUInt32(data, 51);  // [51 52 53 54] (4 bytes)
            MTU = BitConverter.ToUInt32(data, 55);  // [55 56 57 58] (4 bytes)
            TYPE = BitConverter.ToUInt32(data, 59);  // [59 60 61 62] (4 bytes)
            SOCKET_ID = BitConverter.ToUInt32(data, 63);  // [63 64 65 66] (4 bytes)
            SYN_COOKIE = BitConverter.ToUInt32(data, 67);  // [67 68 69 70] (4 bytes)
            PEER_IP = new IpV4Address(BitConverter.ToUInt32(data, 71));  // [71 72 73 74] (4 bytes)

            PEER_IP = new IpV4Address(MethodExt.ReverseIp(PEER_IP.ToString()));  // Reverse the ip because the little/big endian
        }


        /// <summary>
        /// The function checks if it's a handshake packet
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if handshake, false if not</returns>
        public static bool IsHandshake(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.HANDSHAKE;
        }

        /// <summary>
        /// 32 bits (4 bytes). A base protocol version number. Currently used
        /// values are 4 and 5. Values greater than 5 are reserved for future
        /// use.
        /// </summary>
        public uint VERSION { get; private set; }

        /// <summary>
        /// 16 bits (2 bytes). Block cipher family and key size. The
        /// values of this field are described in Table 2. The default value
        /// is AES-128.
        /// </summary>
        public ushort ENCRYPTION_TYPE { get; private set; }

        /// <summary>
        /// 256 bits (32 bytes). Fixed size which is Diffie-Hellman key-exchange method use.
        /// NOTICE: if there is no encryption at all, the encryption will be fulled '0' bytes ({0x0, 0x0, 0x0, ...}) as well.
        /// </summary>
        public byte[] ENCRYPTION_PUBLIC_KEY { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). The sequence number of the
        /// very first data packet to be sent.
        /// INITIAL PACKET SEQUENCE NUMBER, shortened: INITIAL_PSN
        /// </summary>
        public uint INTIAL_PSN { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). This value is typically set
        /// to 1500, which is the default Maximum Transmission Unit(MTU) size
        /// for Ethernet, but can be less.
        /// </summary>
        public uint MTU { get; private set; }  // Maximum Transmission Unit Size

        /// <summary>
        /// 32 bits (4 bytes). This field indicates the handshake packet
        /// type.
        /// </summary>
        public uint TYPE { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). This field holds the ID of the source SRT
        /// socket from which a handshake packet is issued.
        /// </summary>
        public uint SOCKET_ID { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). Randomized value for processing a 
        /// The value of this field is specified by the handshake message
        /// type.
        /// </summary>
        public uint SYN_COOKIE { get; private set; }

        /// <summary>
        /// 32 bits (4 bytes). IPv4 address of the packet's
        /// sender.The value consists of four 32-bit fields.In the case of
        /// IPv4 addresses, fields 2, 3 and 4 are filled with zeroes.
        /// </summary>
        public IpV4Address PEER_IP { get; private set; }

        public enum HandshakeType : uint
        {
            DONE = 0xFFFFFFFD,
            AGREEMENT = 0xFFFFFFFE,
            CONCLUSION = 0xFFFFFFFF,
            WAVEHAND = 0x00000000,
            INDUCTION = 0x00000001
        }

        public override string ToString()
        {
            string handshake = "";

            handshake += "Source SId: " + SOCKET_ID + "\n";
            handshake += "Dest SId: " + DEST_SOCKET_ID + "\n";
            handshake += "Cookie: " + SYN_COOKIE + "\n";
            handshake += "Peer ip: " + PEER_IP.ToString() + "\n";
            handshake += "Handshake type: " + TYPE.ToString("X") + "\n";
            handshake += "Encryption type: " + ((EncryptionType)ENCRYPTION_TYPE).ToString() + "\n";
            handshake += "Encryption public key: " + BitConverter.ToString(ENCRYPTION_PUBLIC_KEY) + "\n";
            handshake += "Initial PSN: " + INTIAL_PSN;

            return handshake;
        }
    }
}
