using SRTShareLib;
using SRTShareLib.PcapManager;
using System;
using System.IO;
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
            CColorManager.WriteLine("\t-- SRT Client  --\n", MessageType.txtWarning);
            Console.Title = "SRT Client";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;  // to handle libraries missing

            _ = ConfigManager.IP;

            NetworkManager.PrintInterfaceData();

            Application.Run(new MainMenu());
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            if (ex is FileNotFoundException || ex.InnerException is FileNotFoundException)
            {
                MessageBox.Show("File PcapDotNet.Core.dll couldn't be found or one of its dependencies. Make sure you have installed:\n" +
                "- .NET Framework 4.5\n" +
                "- WinPcap\n" +
                "- Microsoft Visual C++ 2013\n", "File not found (probably libraries issue)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }
}
