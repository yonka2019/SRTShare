﻿using Newtonsoft.Json;
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
    /// [ATTENTION]  This config file refers only to SERVER SIDE (client side uses Settings.settings configuration after GUI update)
    /// Client side uses this class (ConfigManager) only for getting the IP & PORT properties.
    /// </summary>
    public static class ConfigManager
    {
        private const bool ALWAYS_CREATE_NEW = false;  // even if the config exist - create a new one and overwrite the old one, if it's false, it will take the last created config (if exists)

        internal const string CONFIG_NAME = "SRT_ServerSettings.json";
        public static string IP { get; private set; }
        public static ushort PORT { get; private set; }

        private static readonly string configDirectory = null;
        private static readonly App calledFrom;

        static ConfigManager()
        {
            calledFrom = CalledFrom();

            if (calledFrom == App.Server)
            {
                configDirectory = FindDirectoryWithFile(Directory.GetCurrentDirectory(), CONFIG_NAME);

                if (configDirectory == null || ALWAYS_CREATE_NEW)
                {
                    if (ALWAYS_CREATE_NEW)
                        CConsole.WriteLine("[ERROR] Always create new configuration file flag enabled\n" +
                                            "To create your own config file, press [C] key to create it, or any another key to exit", MessageType.txtWarning);
                    else
                        CConsole.WriteLine("[ERROR] Can't find config file\n" +
                                            "To create your own config file, press [C] key to create it, or any another key to exit", MessageType.txtWarning);

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

                // search again for the settings file in the directories
                configDirectory = FindDirectoryWithFile(Directory.GetCurrentDirectory(), CONFIG_NAME);

                string json = File.ReadAllText($"{configDirectory}\\{CONFIG_NAME}");

                // Deserialize the JSON string into a Person object
                dynamic server = JsonConvert.DeserializeObject(json);

                SetData(Convert.ToString(server.IP), Convert.ToUInt16(server.PORT));
            }
        }

        public static void SetData(string ip, ushort port)
        {
            IP = ip;
            PORT = port;
        }

        private static void CreateConfig(string ip, string port)
        {
            string json = "{\n" +
                $"\"IP\": \"{ip}\",\n" +
                $"\"PORT\": {port}\n" +
                "}\n";

            File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + CONFIG_NAME, json);  // writes json data to file (if file already exists (to handle with ALWAYS_CREATE_NEW flag), file will be overwritten)
        }

        private static void GetConfigData(out string IP, out string port)
        {
            Regex IPRegex = new Regex(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$");
            Regex portRegex = new Regex(@"^\d{1,5}$");

            Console.Clear();

            NetworkManager.PrintInterfaceData();

            Console.WriteLine($"# --- Creating config ({CONFIG_NAME}) --- #\n");

            IP = GetIP(IPRegex);

            Console.WriteLine();

            port = GetPort(portRegex);

            Console.WriteLine("# --- ---- ---- ----- ---- ---- --- #\n");
        }

        /// <summary>
        /// Gets the IP from the user according the IP regex pattern
        /// </summary>
        /// <param name="IPRegex">IP regex pattern</param>
        /// <returns>IP which meets all pattern requirements</returns>
        private static string GetIP(Regex IPRegex)
        {
            bool IPGood = false;
            string IP = null;

            while (!IPGood)
            {
                if (calledFrom == App.Server)
                    Console.WriteLine("* Put here your own LOCAL IP (LAN). Even if you are using external connection.\n[OR] you can input \"my\" to auto-set your local ip.\n");

                Console.Write(">> Server IP: ");
                IP = Console.ReadLine();

                if (IP.ToLower() == "my")
                {
                    IP = NetworkManager.LocalIp;  // auto set local ip
                }

                Match ipMatch = IPRegex.Match(IP);  // 1st check [num.num.num.num]
                IPGood = ipMatch.Success;

                if (IPGood)
                {  // 2nd check [0-255.0-255.0-255.0-255]
                    for (int i = 1; i <= 4; i++)
                    {
                        if (int.Parse(ipMatch.Groups[i].Value) < 0 || int.Parse(ipMatch.Groups[i].Value) > 255)  // check if each block is between 0-255
                        {
                            IPGood = false;
                            break;
                        }
                    }
                }

                // ## NOT IP -> MAYBE HOSTNAME ##
                if (!IPGood)  // trying to send DNS request to get the IP
                {
                    CConsole.Write("[DNS Request] ", MessageType.txtWarning);
                    CConsole.WriteLine("Please wait..\n", MessageType.txtMuted);

                    string hostName = IP;
                    IP = NetworkManager.DnsRequest(hostName);

                    if (IP == null)  // can't find IP
                        // an issue could be occured because a wrong hostname, or some issues with the DNS request.. preferably to set IP address
                        CConsole.WriteLine($"Can't get back DNS reply for the hostname: '{hostName}' (maybe wrong hostname)\n", MessageType.txtError);

                    else  // IP found
                    {
                        CConsole.WriteLine("[DNS Reply]", MessageType.txtWarning);
                        Console.WriteLine($"Hostname: {hostName}\n" +
                                          $"IP Address: {IP}");

                        // If the given server ip is the client external ip, it means that the server is in the same subnet within the clients' subnet. And he should input the local one to avoid loop in the server

                        /* -- Full explanation --
                         * The loop can occur if the server tries to respond to the client by sending the response back to the client's external IP address, which is the same as the server IP address received in the request.

                            In this scenario, the response from the server will be sent to the default gateway (router) instead of being sent directly to the client. The router will then forward the response back to the client, but since the response has the same IP address as the original request, the server will again receive the response and send it back to the router. This process will repeat indefinitely, creating a loop.
                         */
                        if (IP == NetworkManager.PublicIp)
                        {
                            Console.WriteLine();
                            CConsole.Write("Bad IP  ", MessageType.txtError);
                            CConsole.WriteLine("Please specify the local IP of the server\n", MessageType.txtMuted);
                        }
                        else
                            IPGood = true;  // found IP address, and the ip is good (not the same as the public one)
                    }
                }
            }
            return IP;
        }

        /// <summary>
        /// Gets the port from the user according the port regex pattern
        /// </summary>
        /// <param name="portRegex">port regex pattern</param>
        /// <returns>port which meets all pattern requirements</returns>
        private static string GetPort(Regex portRegex)
        {
            bool portGood = false;
            string port = null;

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
            return port;
        }

        /// <summary>
        /// Get the directory which contains the settings file
        /// </summary>
        /// <param name="currentDirectory">current directory (on function calling, it's the current dir)</param>
        /// <param name="fileName">name of the configuration file (*.json)</param>
        /// <returns>directory which contains the config file</returns>
        private static string FindDirectoryWithFile(string currentDirectory, string fileName)
        {
            if (Directory.GetFiles(currentDirectory, fileName).Length == 1)  // in the current directory
                return currentDirectory;

            // not in current -> maybe in the parent directory?
            DirectoryInfo upDir = Directory.GetParent(currentDirectory);
            if (upDir == null)  // there is no parent directory, and the settings still wasn't found -> not exist / duplicated (which is not really possible to be)
                return null;

            string upDirName = upDir.FullName;
            return Directory.GetFiles(upDirName, fileName).Length == 1 ? upDirName : FindDirectoryWithFile(upDirName, fileName);
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
