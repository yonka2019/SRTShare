using System;

namespace SRTManager.ProtocolFields
{
    public class Handshake : SRTHeader
    {
        /// <summary>
        /// Fields -> List<Byte[]> (To send)
        /// </summary>
        public Handshake(uint version, ushort encryption_field, uint intial_psn, uint type, uint source_socket_id, uint dest_socket_id, uint syn_cookie, double p_ip) : base(PacketType.HANDSHAKE, dest_socket_id)
        {
            VERSION = version; byteFields.Add(BitConverter.GetBytes(VERSION));
            ENCRYPTION_FIELD = encryption_field; byteFields.Add(BitConverter.GetBytes(ENCRYPTION_FIELD));
            INTIAL_PSN = intial_psn; byteFields.Add(BitConverter.GetBytes(INTIAL_PSN));
            byteFields.Add(BitConverter.GetBytes(MTU));
            byteFields.Add(BitConverter.GetBytes(MFW));
            TYPE = type; byteFields.Add(BitConverter.GetBytes(TYPE));
            SOCKET_ID = source_socket_id; byteFields.Add(BitConverter.GetBytes(SOCKET_ID));
            SYN_COOKIE = syn_cookie; byteFields.Add(BitConverter.GetBytes(SYN_COOKIE));
            PEER_IP = p_ip; byteFields.Add(BitConverter.GetBytes(Convert.ToDouble(PEER_IP)));
        }

     
        //public ushort CONTROL_TYPE { get; set; }


        //public ushort SUB_TYPE { get; set; }

 
        //public uint TYPE_SPECIFIC_INFO;  // Change later (no use in HandShake)


        //public uint DEST_SOCKET_ID { get; set; }

        public Handshake(byte[] data) : base(PacketType.HANDSHAKE, BitConverter.ToUInt32(data,8))
        {
            CONTROL_TYPE = BitConverter.ToUInt16(data, 0);
            SUB_TYPE = BitConverter.ToUInt16(data, 2);
            TYPE_SPECIFIC_INFO = BitConverter.ToUInt32(data, 4);
            DEST_SOCKET_ID = BitConverter.ToUInt32(data, 8);


            VERSION = BitConverter.ToUInt32(data, 12);  // [0 1 2 3] (4 bytes)
            ENCRYPTION_FIELD = BitConverter.ToUInt16(data, 16);  // [4 5] (2 bytes)
            INTIAL_PSN = BitConverter.ToUInt32(data, 18);  // [6 7 8 9] (4 bytes)
            // MTU = [10 11 12 13] (4 bytes)
            // MFW = [14 15 16 17] (4 bytes)
            TYPE = BitConverter.ToUInt32(data, 30);  // [18 19 20 21] (4 bytes)
            SOCKET_ID = BitConverter.ToUInt32(data, 34);  // [22 23 24 25] (4 bytes)
            SYN_COOKIE = BitConverter.ToUInt32(data, 38);  // [26 27 28 29] (4 bytes)
            PEER_IP = BitConverter.ToUInt64(data, 42);  // [30 31 32 33 34 35 36 38] (8 bytes)
        }

        /// <summary>
        /// 32 bits (4 bytes). A base protocol version number. Currently used
        /// values are 4 and 5. Values greater than 5 are reserved for future
        /// use.
        /// </summary>
        public uint VERSION { get; set; }

        /// <summary>
        /// 16 bits (2 bytes). Block cipher family and key size. The
        /// values of this field are described in Table 2. The default value
        /// is AES-128.
        /// </summary>
        public ushort ENCRYPTION_FIELD { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). The sequence number of the
        /// very first data packet to be sent.
        /// </summary>
        public uint INTIAL_PSN { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). This value is typically set
        /// to 1500, which is the default Maximum Transmission Unit(MTU) size
        /// for Ethernet, but can be less.
        /// </summary>
        public uint MTU => 1000;  // Maximum Transmission Unit Size

        /// <summary>
        /// 32 bits (4 bytes). The value of this field is the
        /// maximum number of data packets allowed to be "in flight" (i.e.the
        /// number of sent packets for which an ACK control packet has not yet
        /// been received).
        /// </summary>
        public uint MFW => 8192; // Maximum Flow Windows Size

        /// <summary>
        /// 32 bits (4 bytes). This field indicates the handshake packet
        /// type.
        /// </summary>
        public uint TYPE { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). This field holds the ID of the source SRT
        /// socket from which a handshake packet is issued.
        /// </summary>
        public uint SOCKET_ID { get; set; }

        /// <summary>
        /// 32 bits (4 bytes). Randomized value for processing a handshake.
        /// The value of this field is specified by the handshake message
        /// type.
        /// </summary>
        public uint SYN_COOKIE { get; set; }

        /// <summary>
        /// 64 bits (8 bytes). IPv4 or IPv6 address of the packet's
        /// sender.The value consists of four 32-bit fields.In the case of
        /// IPv4 addresses, fields 2, 3 and 4 are filled with zeroes.
        /// </summary>
        public double PEER_IP { get; set; }


        public enum Extension // Extension Field
        {
            HSREQ = 0x00000001,
            KMREQ = 0x00000002,
            CONFIG = 0x00000004
        }

        public enum HandshakeType : uint
        {
            DONE = 0xFFFFFFFD,
            AGREEMENT = 0xFFFFFFFE,
            CONCLUSION = 0xFFFFFFFF,
            WAVEHAND = 0x00000000,
            INDUCTION = 0x00000001
        }

        public enum Encryption // Encryption Field
        {
            None = 0,
            AES128 = 2,
            AES192 = 3,
            AES256 = 4
        }
    }
}
