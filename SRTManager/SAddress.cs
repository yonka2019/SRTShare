﻿namespace SRTManager
{
    public class SAddress
    {
        public string IPAddress { get; set; }
        public ushort Port { get; set; }

        public SAddress(string ipAddress, string port = "")  // ["127.0.0.1", "58526"]  (ip, port)
        {
            IPAddress = ipAddress;
            Port = IsDigitsOnly(port) ? ushort.Parse(port) : (ushort)0;
        }

        public SAddress(string ipAddress, int port)  // ["127.0.0.1", 58526]  (ip, port)
        {
            IPAddress = ipAddress;
            Port = (ushort)port;
        }

        public SAddress(string ipAddress, ushort port)  // ["127.0.0.1", 58526]  (ip, port)
        {
            IPAddress = ipAddress;
            Port = port;
        }

        public SAddress(uint ipAddress, ushort port)  // [(byted-ip), 58526]  (ip, port)
        {
            IPAddress = ipAddress.ToString();
            Port = port;
        }

        public SAddress(string socketAddress)  // ["127.0.0.1:58526"]  (ip:port)
        {
            if (socketAddress.Contains(":"))
            {
                string[] _socketAddress = socketAddress.Split(':');

                IPAddress = _socketAddress[0];
                Port = IsDigitsOnly(_socketAddress[1]) ? ushort.Parse(_socketAddress[1]) : (ushort)0;
            }
            else  // ["127.0.0.1"]  (only ip)
            {
                IPAddress = socketAddress;
                Port = 0;
            }
        }

        public string GetSocketAddress()  // combine to "IPAddress:Port" 
        {
            return IPAddress + Port.ToString();
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}