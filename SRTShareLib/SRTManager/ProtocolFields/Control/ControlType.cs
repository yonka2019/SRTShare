namespace SRTShareLib.SRTManager.ProtocolFields.Control
{
    public enum ControlType : ushort
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
        QUALITY_CONTROL = 0x0009,
        USER_DEFINED = 0x7FFF
    }
}
