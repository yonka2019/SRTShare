using SRTShareLib;
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
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _ = ConfigManager.IP;

            PacketManager.PrintInterfaceData();
            PacketManager.PrintServerData();

            TestConnection();

            Application.Run(new MainView());
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("File PcapDotNet.Core.dll couldn't be found or one of its dependencies. Make sure you have installed:\n" +
                "- .NET Framework 4.5\n" +
                "- WinPcap\n" +
                "- Microsoft Visual C++ 2013\n", "Libraries issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(-1);
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
