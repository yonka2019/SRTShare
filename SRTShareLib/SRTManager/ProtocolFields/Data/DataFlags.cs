namespace SRTShareLib.SRTManager.ProtocolFields.Data
{
    public enum PositionFlags : ushort
    {
        FIRST = 0x010b,
        MIDDLE = 0x000b,
        LAST = 0x001b,
        SINGLE_DATA_PACKET = 0x011b
    }

    public enum EncryptionFlags : byte
    {
        NOT_ENCRYPTED = 0,
        ENCRYPTED = 1
    }
}
