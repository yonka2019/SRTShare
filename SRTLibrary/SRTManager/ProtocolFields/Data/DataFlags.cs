using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTLibrary.SRTManager.ProtocolFields.Data
{
    public enum PositionFlags : ushort
    {
        FIRST = 0x010b,
        MIDDLE = 0x000b,
        LAST = 0x001b,
        SINGLE_DATA_PACKET = 0x011b
    }

    public enum EncryptionFlags : ushort
    {
        NOT_ENCRYPTED = 0x000b,
        EVEN_KEY = 0x001b,
        ODD_KEY = 0x010b,
    }
}
