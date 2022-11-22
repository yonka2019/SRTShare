namespace SRTManager
{
    public class ProtocolFields
    {
        public class Handshake
        {
            public Handshake(uint version, ushort encryption_field, ushort extension_field, uint intial_psn,
                uint mtu, uint mfws, uint type, uint socket_id, string syn_cookie, decimal p_ip)
            {
                VERSION = version;
                ENCRYPTION_FIELD = encryption_field;
                EXTENSION_FIELD = extension_field;
                INTIAL_PSN = intial_psn;
                MTU = mtu;
                MFWS = mfws;
                TYPE = type;
                SOCKET_ID = socket_id;
                SYN_COOKIE = syn_cookie;
                PEER_IP = p_ip;
            }

            public uint VERSION { get; set; }
            public ushort ENCRYPTION_FIELD { get; set; }
            public ushort EXTENSION_FIELD { get; set; }
            public uint INTIAL_PSN { get; set; }
            public uint MTU { get; set; }
            public uint MFWS { get; set; }
            public uint TYPE { get; set; }
            public uint SOCKET_ID { get; set; }
            public string SYN_COOKIE { get; set; }
            public decimal PEER_IP { get; set; }

            
            public enum Extension
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

            public enum Encryption
            {
                None = 0,
                AES128 = 2,
                AES192 = 3,
                AES256 = 4
            }
        }

        public enum PacketType
        {
            HANDSHAKE = 0x0000,
            KEEPALIVE = 0x0001,
            ACK = 0x0002,
            NAK = 0x0003,
            CON_WARNING = 0x0004,
            SHUTDOWN = 0x0005,
            ACKACK = 0x0006,
            DROPREQ = 0x0007,
            PEERERROR = 0x0008,
            USER_DEFINED = 0x7FFF
        }
    }
}
