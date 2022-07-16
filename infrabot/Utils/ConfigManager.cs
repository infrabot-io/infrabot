using System;
using System.IO;
using System.Text.Json;
using infrabot.Serialization;

namespace infrabot.Utils
{
    public class ConfigManager
    {
        public Config Config = null;

        public ConfigManager()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            // If config.json file does not exist then exit application
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "config.json"))
            {
                Console.WriteLine("File \"" + AppDomain.CurrentDomain.BaseDirectory + "config.json" + "\" was not found. Please check if this file exists!");
                Environment.Exit(0);
            }

            try
            {
                // Get contents of config.json file
                string jsonConfigFile = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "config.json");

                // Deserialize it
                Config = JsonSerializer.Deserialize<Config>(jsonConfigFile);
            }
            catch (Exception ex)
            {
                // If file is not a valid json file then exit application
                Console.WriteLine("File \"" + AppDomain.CurrentDomain.BaseDirectory + "config.json" + "\" is not a valid configuration file! Error: " + ex.Message);
                Environment.Exit(0);
            }
        }
    }
}
