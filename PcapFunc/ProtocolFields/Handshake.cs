using System;

namespace SRTManager.ProtocolFields
{
    public class Handshake : Header
    {
        public Handshake(uint version, ushort encryption_field, uint intial_psn, uint type, uint socket_id, uint syn_cookie, decimal p_ip)
        {
            VERSION = version; byteFields.Add(BitConverter.GetBytes(VERSION));
            ENCRYPTION_FIELD = encryption_field; byteFields.Add(BitConverter.GetBytes(ENCRYPTION_FIELD));
            INTIAL_PSN = intial_psn; byteFields.Add(BitConverter.GetBytes(INTIAL_PSN));
            byteFields.Add(BitConverter.GetBytes(MTU));
            byteFields.Add(BitConverter.GetBytes(MFW));
            TYPE = type; byteFields.Add(BitConverter.GetBytes(TYPE));
            SOCKET_ID = socket_id; byteFields.Add(BitConverter.GetBytes(SOCKET_ID));
            SYN_COOKIE = syn_cookie; byteFields.Add(BitConverter.GetBytes(SYN_COOKIE));
            PEER_IP = p_ip; byteFields.Add(BitConverter.GetBytes(Convert.ToDouble(PEER_IP)));
        }

        public uint VERSION { get; set; }
        public ushort ENCRYPTION_FIELD { get; set; }
        public uint INTIAL_PSN { get; set; }
        public uint MTU => 1000;  // Maximum Transmission Unit Size
        public uint MFW => 8192; // Maximum Flow Windows Size
        public uint TYPE { get; set; }
        public uint SOCKET_ID { get; set; }
        public uint SYN_COOKIE { get; set; }
        public decimal PEER_IP { get; set; }


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
