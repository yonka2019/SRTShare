using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using CConsole = SRTShareLib.CColorManager;

namespace SRTShareLib.PcapManager
{
    public class NetworkManager  // Network Interface Manager
    {
        public static readonly LivePacketDevice Device;  // active network interface (in global usage)

        public static readonly string LocalIp;
        public static readonly string PublicIp;
        public static readonly string MacAddress;
        public static readonly string DefaultGateway;
        public static string Mask { get; private set; }

        static NetworkManager()
        {
            LocalIp = GetActiveLocalIp();
            PublicIp = GetActivePublicIp();
            Device = AutoSelectNetworkInterface(LocalIp);
            MacAddress = Device.GetMacAddress().ToString().Replace("-", ":");
            DefaultGateway = Device.GetNetworkInterface().GetIPProperties().GatewayAddresses.Where(inter => inter.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First().Address.ToString();
        }

        public static void PrintInterfaceData()
        {
            Console.WriteLine($"####################\n[!] SELECTED INTERFACE: {Device.Description}\n" +
                            $"* Local IP: {LocalIp}\n" +
                            $"* Public IP: {PublicIp}\n" +
                            $"* MAC: {MacAddress}\n" +
                            $"* Gateway: {DefaultGateway}\n" +
                            $"* Mask: {Mask}\n" +
                            $"####################\n\n");
        }

        public static void PrintServerData()
        {
            Console.WriteLine($"####################\n[!] SERVER SETTINGS (from {ConfigManager.CONFIG_NAME})\n" +
                            $"* IP: {ConfigManager.IP}\n" +
                            $"* PORT: {ConfigManager.PORT}\n" +
                            $"####################\n\n");
        }

        /// <summary>
        /// The function gets the local ip of the computer
        /// </summary>
        /// <returns>The computer's local ip</returns>
        private static string GetActiveLocalIp()
        {
            IPAddress localAddress = null;
            string googleDns = "8.8.8.8";

            try
            {
                UdpClient u = new UdpClient(googleDns, 1);
                localAddress = ((IPEndPoint)u.Client.LocalEndPoint).Address;
            }
            catch
            {
                CConsole.WriteLine("[ERROR] Can't find local IP", MessageType.bgError);  // there is no valid NI (Network Interface)
                Console.WriteLine("Press any key to continue...");

                Console.ReadKey();
                Environment.Exit(-1);
            }

            return localAddress.ToString();
        }

        #region https://stackoverflow.com/questions/3253701/get-public-external-ip-address
        private static string GetActivePublicIp()
        {
            int errorCounter = 0;
            string publicIp = null;
            string checkIpURL = @"http://checkip.dyndns.org";

            string response;
            string[] a;
            string a2;
            string[] a3;

            while (publicIp == null && errorCounter < 3)  // 3 tries
            {
                try
                {
                    WebRequest req = WebRequest.Create(checkIpURL);
                    WebResponse resp = req.GetResponse();
                    System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

                    response = sr.ReadToEnd().Trim();
                    a = response.Split(':');
                    a2 = a[1].Substring(1);
                    a3 = a2.Split('<');
                    publicIp = a3[0];
                }
                catch
                {
                    errorCounter++;
                    publicIp = null;
                }
            }

            if (publicIp == null)  // still null
            {
                CConsole.Write("[ERROR] Can't find public IP, ", MessageType.bgError);  // no external connection, only LAN supported (public ip switched with the local ip)
                CConsole.WriteLine("using only LAN supported connection (public ip changed with local ip)", MessageType.txtError);
                return LocalIp;
            }

            return publicIp;
        }
        #endregion

        /// <summary>
        /// The function auto selects the device where all the messages will be sent to
        /// </summary>
        /// <param name="activeLocalIp">Local ip</param>
        /// <returns>Device where all the messages will be sent to</returns>
        private static LivePacketDevice AutoSelectNetworkInterface(string activeLocalIp)
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            int selectDeviceIndex = -1;

            // iterate interfaces list and found the right one
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                foreach (DeviceAddress deviceAddress in device.Addresses)
                {
                    if (deviceAddress.Address.ToString().Contains(activeLocalIp))
                    {
                        Mask = deviceAddress.Netmask.ToString().Replace("Internet ", "");

                        selectDeviceIndex = i + 1;
                        break;
                    }
                }
            }

            if (allDevices.Count == 0)
            {
                CConsole.WriteLine("[ERROR] No interfaces found", MessageType.bgError);

                Console.ReadKey();
                Environment.Exit(0);
            }

            if (selectDeviceIndex == -1)
            {
                CConsole.WriteLine($"[ERROR] There is no interface which matches with the local ip address", MessageType.txtError);

                Console.ReadKey();
                Environment.Exit(0);
            }

            // Take the selected adapter
            return allDevices[selectDeviceIndex - 1];
        }
    }
}
