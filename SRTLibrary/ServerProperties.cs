using Newtonsoft.Json;
using System.IO;

namespace SRTLibrary
{
    /// <summary>
    /// This class manage the server IP/PORT according the information which stored in the settings.json 
    /// file which is can be in the one of directories (or parent-directories) of the runnning .exe (even in the root dir)
    /// </summary>
    public static class ServerProperties
    {
        private const string CONFIG_NAME = "settings.json";
        public static string IP { get; private set; }
        public static ushort PORT { get; private set; }

        static ServerProperties()
        {
            // Read the JSON file into a string
            string ap = UpDirTo(Directory.GetCurrentDirectory(), CONFIG_NAME);
            if (ap == null)
            {
                System.Console.WriteLine("[ERROR] Can't find config file");
                System.Console.ReadKey();
                System.Environment.Exit(-1);
            }

            string json = File.ReadAllText($"{ap}\\{CONFIG_NAME}");

            // Deserialize the JSON string into a Person object
            dynamic server = JsonConvert.DeserializeObject(json);

            IP = server.IP;
            PORT = server.PORT;
        }

        /// <summary>
        /// Get the directory which contains the settings file
        /// </summary>
        /// <param name="currentDirectory">current directory (on function calling, it's the current dir)</param>
        /// <param name="settingsFileName">name of the configuration file (*.json)</param>
        /// <returns>directory which contains the config file</returns>
        private static string UpDirTo(string currentDirectory, string settingsFileName)
        {
            DirectoryInfo upDir = Directory.GetParent(currentDirectory);
            if (upDir == null)
                return null;

            string upDirName = upDir.FullName;

            return Directory.GetFiles(upDirName, settingsFileName).Length != 1 ? UpDirTo(upDirName, settingsFileName) : upDirName;
        }
    }
}
