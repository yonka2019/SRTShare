using System.Collections.Generic;

namespace SRTManager.ProtocolFields
{
    public class SRTHeader
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }


        public SRTHeader()
        {

        }
    }
}
