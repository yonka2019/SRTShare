using System;

namespace SRTManager.ProtocolFields.Control
{
    public class Handshake : SRTHeader
    {
        public Handshake(uint version, ushort encryption_field, uint intial_psn, uint type, uint source_socket_id, uint dest_socket_id, uint syn_cookie, double p_ip) : base(ControlType.HANDSHAKE, dest_socket_id)
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

        public Handshake(byte[] data) : base(data)  // initialize SRT Control header fields
        {
            // initialize SRT Control Handshake header fields

            VERSION = BitConverter.ToUInt32(data, 13);  // [13 14 15 16] (4 bytes)
            ENCRYPTION_FIELD = BitConverter.ToUInt16(data, 17);  // [17 18] (2 bytes)
            INTIAL_PSN = BitConverter.ToUInt32(data, 19);  // [19 20 21 22] (4 bytes)
            // MTU = [23 24 25 26] (4 bytes)
            // MFW = [27 28 29 30] (4 bytes)
            TYPE = BitConverter.ToUInt32(data, 31);  // [31 32 33 34] (4 bytes)
            SOCKET_ID = BitConverter.ToUInt32(data, 35);  // [35 36 37 38] (4 bytes)
            SYN_COOKIE = BitConverter.ToUInt32(data, 39);  // [39 40 41 42] (4 bytes)
            PEER_IP = BitConverter.ToUInt64(data, 43);  // [43 44 45 46 47 48 49 50] (8 bytes)
        }

        public static bool IsHandshake(byte[] data)
        {
            return BitConverter.ToUInt16(data, 1) == (ushort)ControlType.HANDSHAKE;
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
        /// 32 bits (4 bytes). Randomized value for processing a 
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
