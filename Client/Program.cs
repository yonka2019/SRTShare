using SRTLibrary;
using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _ = ConfigManager.IP;
            TestConnection();

            PacketManager.PrintInterfaceData();
            PacketManager.PrintServerData();

            Application.Run(new MainView());
        }

        /// <summary>
        /// Before the SRT connection, check via ICMP (ping) if the server totally alive
        /// </summary>
        private static void TestConnection()
        {
            Ping ping = new Ping();

            // Set the address to ping
            string ipAddress = ConfigManager.IP;

            // Send the ping and get the reply
            PingReply reply = ping.Send(ipAddress);

            // Check the status of the reply
            if (reply.Status != IPStatus.Success)
            {
                MessageBox.Show("Server isn't alive\n..or he doesn't allowed to receive any ICMP requests", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }
}
