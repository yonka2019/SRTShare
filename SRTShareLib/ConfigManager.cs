using Newtonsoft.Json;
using SRTShareLib.PcapManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using CConsole = SRTShareLib.CColorManager;

namespace SRTShareLib
{
    /// <summary>
    /// This class manage the server IP/PORT according the information which stored in the settings.json 
    /// file which is can be in the one of directories (or parent-directories) of the runnning .exe (even in the root dir)
    /// </summary>
    public static class ConfigManager
    {
        public const string CONFIG_NAME = "settings.json";
        public static string IP { get; private set; }
        public static ushort PORT { get; private set; }

        private static readonly string configDirectory = null;
        private static readonly App calledFrom;

        static ConfigManager()
        {
            calledFrom = CalledFrom();

            while (configDirectory == null)
            {
                // Read the JSON file into a string
                configDirectory = UpDirTo(Directory.GetCurrentDirectory(), CONFIG_NAME);
                if (configDirectory == null)
                {
                    CConsole.WriteLine("[ERROR] Can't find config file (or maybe he is duplicated at the same directory?)\n" +
                        "You can create your own config file, press [C] key to create it, or any another key to exit", MessageType.txtWarning);

                    if (Console.ReadKey().Key == ConsoleKey.C)
                    {
                        GetConfigData(out string ip, out string port);

                        CreateConfig(ip, port);

                        Console.WriteLine("Config file successfully created!\n" +
                            "Press any key to restart the application...");
                        Console.ReadKey();

                        Console.Clear();
                    }
                    else
                        Environment.Exit(-1);
                }
            }
            string json = File.ReadAllText($"{configDirectory}\\{CONFIG_NAME}");

            // Deserialize the JSON string into a Person object
            dynamic server = JsonConvert.DeserializeObject(json);

            IP = server.IP;
            PORT = server.PORT;
        }

        private static void CreateConfig(string ip, string port)
        {
            string json = "{\n" +
                $"\"IP\": \"{ip}\",\n" +
                $"\"PORT\": {port}\n" +
                "}\n";

            File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + CONFIG_NAME, json);
        }

        private static void GetConfigData(out string ip, out string port)
        {
            ip = "";
            Regex ipRegex = new Regex(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$");
            bool ipAddress = false;

            port = "";
            Regex portRegex = new Regex(@"^\d{1,6}$");
            bool portGood = false;

            Console.Clear();

            NetworkManager.PrintInterfaceData();

            Console.WriteLine($"# --- Creating config ({CONFIG_NAME}) --- #\n");

            // get IP
            while (!ipAddress)
            {
                if (calledFrom == App.Server)
                    Console.WriteLine("* Put here your own LOCAL IP (LAN) even if you are using external connection\n[OR] you can input \"my\" to auto-set your local ip.\n");

                else if (calledFrom == App.Client)
                    Console.WriteLine("* If your server in the same subnet with the client\n" +
                    "put here the local ip of the server (LAN), either, put the public one (WAN).\n" +
                    "In addition, you can also put here a hostname (DNS Supported)");

                Console.Write(">> Server IP: ");
                ip = Console.ReadLine();

                if (ip.ToLower() == "my")
                {
                    ip = NetworkManager.LocalIp;  // auto set local ip
                }

                Match ipMatch = ipRegex.Match(ip);  // 1st check [num.num.num.num]
                ipAddress = ipMatch.Success;

                if (ipAddress)
                {  // 2nd check [0-255.0-255.0-255.0-255]
                    for (int i = 1; i <= 4; i++)
                    {
                        if (int.Parse(ipMatch.Groups[i].Value) < 0 || int.Parse(ipMatch.Groups[i].Value) > 255)  // check if each block is between 0-255
                        {
                            ipAddress = false;
                            break;
                        }
                    }
                }

                if (!ipAddress)  // maybe it's hostname, trying to send DNS request to get the IP
                {
                    // TODO: CHECK DNS AND GET REAL IP AND SET IT
                }
            }

            Console.WriteLine();

            // get Port
            while (!portGood)
            {
                if (calledFrom == App.Server)
                    Console.WriteLine("* Put here any unused port.");

                Console.Write(">> Server Port: ");
                port = Console.ReadLine();  // add regex check

                portGood = portRegex.IsMatch(port);  // 1st check [num]

                if (portGood)
                {  // 2nd check [1-65353]
                    if (int.Parse(port) <= 0 || int.Parse(port) > 65353)
                        portGood = false;
                }

                if (!portGood)
                {
                    CConsole.WriteLine("Bad port\n", MessageType.txtError);
                }
            }
            Console.WriteLine("# --- ---- ---- ----- ---- ---- --- #\n");
        }

        /// <summary>
        /// Get the directory which contains the settings file
        /// </summary>
        /// <param name="currentDirectory">current directory (on function calling, it's the current dir)</param>
        /// <param name="settingsFileName">name of the configuration file (*.json)</param>
        /// <returns>directory which contains the config file</returns>
        private static string UpDirTo(string currentDirectory, string settingsFileName)
        {
            if (Directory.GetFiles(currentDirectory, settingsFileName).Length == 1)  // in the current directory
                return currentDirectory;

            // not in current -> maybe in the parent directory?
            DirectoryInfo upDir = Directory.GetParent(currentDirectory);
            if (upDir == null)  // there is no parent directory, and the settings wasn't found -> not exist/duplicated
                return null;

            string upDirName = upDir.FullName;
            return Directory.GetFiles(upDirName, settingsFileName).Length != 1 ? UpDirTo(upDirName, settingsFileName) : upDirName;
        }

        private static App CalledFrom()
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            MethodBase methodBase = frames[frames.Length - 1].GetMethod();  // get first stack frame (could be only client/server)

            string calledFrom_ProjectName = methodBase.ReflectedType.Namespace;
            App app = (App)Enum.Parse(typeof(App), calledFrom_ProjectName, true);

            return app;
        }
    }

    public enum App
    {
        Client,
        Server,
        SRTShareLib
    }
}
