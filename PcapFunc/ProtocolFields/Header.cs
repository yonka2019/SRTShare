using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTManager.ProtocolFields
{
    public abstract class Header
    {
        protected readonly List<byte[]> byteFields = new List<byte[]>();
        public List<byte[]> GetByted() { return byteFields; }
    }
}
