using System.Collections.Generic;

namespace SRTManager.ProtocolFields
{
    public abstract class Header
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }
    }
}
