namespace SRTLibrary
{
    public class SClient
    {
        public string IPAddress { get; set; }
        public ushort Port { get; set; }
        public string MacAddress { get; set; }

        public SClient(string ipAddress)
        {
            IPAddress = ipAddress;
            Port = 0;
            MacAddress = null;
        }

        public SClient(string ipAddress, int port) : this(ipAddress)
        {
            Port = (ushort)port;
        }

        public SClient(string ipAddress, int port, string macAddress) : this(ipAddress, port)
        {
            MacAddress = macAddress;
        }

        public override string ToString()  // combine to "IPAddress:Port" 
        {
            return IPAddress + Port.ToString();
        }
    }
}
