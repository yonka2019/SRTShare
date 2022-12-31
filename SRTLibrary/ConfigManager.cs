using Newtonsoft.Json;
using PcapDotNet.Packets.Ethernet;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRTLibrary
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

        static ConfigManager()
        {
            while (configDirectory == null)
            {
                // Read the JSON file into a string
                configDirectory = UpDirTo(Directory.GetCurrentDirectory(), CONFIG_NAME);
                if (configDirectory == null)
                {
                    Console.WriteLine("[ERROR] Can't find config file (or maybe he is duplicated at the same directory?)\n" +
                        "You can create your own config file, press [C] key to create it, or any another key to exit");

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
            bool ipGood = false;

            port = "";
            Regex portRegex = new Regex(@"^\d{1,6}$");
            bool portGood = false;

            Console.Clear();

            PacketManager.PrintInterfaceData();

            Console.WriteLine($"# --- Creating config ({CONFIG_NAME}) --- #\n");

            // get IP
            while (!ipGood)
            {
                Console.Write("* If your server in the same subnet with the client\n" +
                    "put here the internal ip (LAN), either, put the external one (WAN).\n" +
                    "* If you are creating config for the server, put here your own IP (lan/wan).\n" +
                    ">> Server IP: ");
                ip = Console.ReadLine();

                Match ipMatch = ipRegex.Match(ip);  // 1st check [num.num.num.num]
                ipGood = ipMatch.Success;

                if (ipGood)
                {  // 2nd check [0-255.0-255.0-255.0-255]
                    for (int i = 1; i <= 4; i++)
                    {
                        if (int.Parse(ipMatch.Groups[1].Value) < 0 || int.Parse(ipMatch.Groups[1].Value) > 255)  // check if each block is between 0-255
                        {
                            ipGood = false;
                            break;
                        }
                    }
                }

                if (!ipGood)
                {
                    Console.WriteLine("Bad IP\n");
                }
            }

            Console.WriteLine();

            // get Port
            while (!portGood)
            {
                Console.Write("* If you are creating config for the server, put here any unused port.\n" +
                    ">> Server Port: ");
                port = Console.ReadLine();  // add regex check
                portGood = portRegex.IsMatch(port);  // 1st check [num]

                if (portGood)
                {  // 2nd check [1-65353]
                    if (int.Parse(port) <= 0 || int.Parse(port) > 65353)
                        portGood = false;
                }

                if (!portGood)
                {
                    Console.WriteLine("Bad port\n");
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
    }
}
