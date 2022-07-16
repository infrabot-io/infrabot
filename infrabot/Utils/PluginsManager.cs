using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Telegram.Bot.Types;
using infrabot.PluginSystem;
using infrabot.PluginSystem.Utils;

namespace infrabot.Utils
{
    public class PluginsManager
    {
        private static FileSystemWatcher Watcher = new FileSystemWatcher();
        public static string PluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";
        public static List<Plugin> Plugins = new List<Plugin>();

        public PluginsManager()
        {
            // Check if plugins folder exist, and if not create it
            if (Directory.Exists(PluginsPath) == false)
            {
                Directory.CreateDirectory(PluginsPath);
            }

            // Watch our plugins path for changes
            Watcher.Path = PluginsPath;

            // Trigger on changes
            Watcher.NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            // React only for .plug file extension
            Watcher.Filter = "*.plug";

            // Perform actions when event happened
            Watcher.Changed += FileOnChanged;
            Watcher.Created += FileOnCreated;
            Watcher.Deleted += FileOnDeleted;
            Watcher.Renamed += FileOnRenamed;

            // Enable our FileSystemWatcher
            Watcher.EnableRaisingEvents = true;

            // Load plugins
            ReloadPlugins();
        }

        private static void FileOnChanged(object source, FileSystemEventArgs e)
        {
            string folderPath = "";

            try
            {
                // We have to set "watcher.EnableRaisingEvents = false" because on each event
                // this part will execute many times. To avoid this behaviour we temporarily 
                // turn it off, and then turn it on again at the end of the execution
                Watcher.EnableRaisingEvents = false;

                // Get Our changed Plugin file
                Plugin plugin = PluginActions.GetPlugin(e.FullPath);

                // Determine full path to extract data
                folderPath = PluginsPath + @"\" + plugin.Guid;

                // Delete our old plugin folder path since our plugin file changed since
                // we dont know if our plugin file is a completeley another plugin file.
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    WaitForDeletion(folderPath);
                }

                // Recreate new folder
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Extract contents of our plugin
                PluginActions.ExtractPluginFiles(plugin, folderPath);
            }
            catch (Exception ex)
            {
                WriteToPluginsLog("FileOnChanged. Could not change plugin in folder: " + folderPath + ". Error was: " + ex.Message);
                WriteToPluginsLog("FileOnChanged. Can not redeploy plugin! File will not be executed.");
                ReloadPlugins(e.FullPath);
                return;
            }
            finally
            {
                // Turn back on event triggering
                Watcher.EnableRaisingEvents = true;
            }

            WriteToPluginsLog("FileOnChanged. Reload plugins event triggered");
            ReloadPlugins();
        }

        private static void FileOnCreated(object source, FileSystemEventArgs e)
        {
            string folderPath = "";

            try
            {
                // Get Our new Plugin file
                Plugin plugin = PluginActions.GetPlugin(e.FullPath);

                // Determine full path
                folderPath = PluginsPath + @"\" + plugin.Guid;

                // Clear Plugin folder if exists
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    WaitForDeletion(folderPath);
                }

                // Recreate new folder for our plugin
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Extract contents of our plugin
                PluginActions.ExtractPluginFiles(plugin, folderPath);
            }
            catch (Exception ex)
            {
                WriteToPluginsLog("FileOnCreated. Could not extract plugin to folder: " + folderPath + ". Error was: " + ex.Message);
                WriteToPluginsLog("FileOnCreated. Can not deploy plugin!");
                return;
            }

            WriteToPluginsLog("FileOnCreated. Reload plugins event triggered");
            ReloadPlugins(); 
        }

        private static void FileOnDeleted(object source, FileSystemEventArgs e)
        {
            WriteToPluginsLog("FileOnDeleted. Reload plugins event triggered");
            ReloadPlugins();
        }

        private static void FileOnRenamed(object source, RenamedEventArgs e)
        {
            WriteToPluginsLog("FileOnRenamed. File has been renamed. Nothig to do though.");
        }

        private static void WaitForDeletion(string directoryName)
        {
            bool deleted = false;
            do
            {
                deleted = !Directory.Exists(directoryName);
                Thread.Sleep(100);
            } while (!deleted);
        }

        public static void WriteToPluginsLog(string Log)
        {
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "logs") == false)
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "logs");
                }
                DateTime localDate = DateTime.Now;
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"logs\plugins.log", localDate.ToString() + ": " + Log + Environment.NewLine);
            }
            catch { }
        }

        public static void ReloadPlugins()
        {
            try
            {
                if (Directory.Exists(PluginsPath))
                {
                    Plugins.Clear();
                    string[] pluginFiles = Directory.GetFiles(PluginsPath, "*.plug", SearchOption.TopDirectoryOnly);

                    if (pluginFiles.Length > 0)
                    {
                        foreach (string pluginFile in pluginFiles)
                        {
                            Plugin plugin = PluginActions.GetPlugin(pluginFile);
                            Plugins.Add(plugin);
                        }
                    }

                    Program.SetMyCommandsForChat(GetMyCommandsForChat());

                    WriteToPluginsLog("Reload plugins. Found plugins count: " + Plugins.Count.ToString());
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog("Error. Could not load plugins: " + ex.Message);
            }
        }

        public static void ReloadPlugins(string ignorePluginFilePath)
        {
            try
            {
                if (Directory.Exists(PluginsPath))
                {
                    Plugins = null;
                    string[] pluginFiles = Directory.GetFiles(PluginsPath, "*.plug", SearchOption.TopDirectoryOnly);

                    if (pluginFiles.Length > 0)
                    {
                        foreach (string pluginFile in pluginFiles)
                        {
                            if(pluginFile == ignorePluginFilePath)
                                continue;

                            Plugin plugin = PluginActions.GetPlugin(pluginFile);
                            Plugins.Add(plugin);
                        }
                    }

                    Program.SetMyCommandsForChat(GetMyCommandsForChat());

                    WriteToPluginsLog("Reload plugins. Found plugins count: " + Plugins.Count.ToString());
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog("Error. Could not load plugins: " + ex.Message);
            }
        }

        public static List<BotCommand> GetMyCommandsForChat()
        {
            List<BotCommand> botCommands = new List<BotCommand>();

            // Add /reloadconfig
            if (Program.ConfigManagerInstance.Config.telegram_enable_reloadconfig == true)
            {
                botCommands.Add(new BotCommand()
                {
                    Command = "/reloadconfig",
                    Description = "Force reload configuration and plugins."
                });
            }

            // Add /emergency
            if (Program.ConfigManagerInstance.Config.telegram_enable_emergency == true)
            {
                botCommands.Add(new BotCommand()
                {
                    Command = "/emergency",
                    Description = "Force shuts down service."
                });
            }

            // Add /showmyid
            if (Program.ConfigManagerInstance.Config.telegram_enable_showmyid == true)
            {
                botCommands.Add(new BotCommand()
                {
                    Command = "/showmyid",
                    Description = "Shows user id"
                });
            }

            // Add /getcommands
            botCommands.Add(new BotCommand()
            {
                Command = "/getcommands",
                Description = "Gets a list of all available commands"
            });

            if (Plugins.Count > 0)
            {
                foreach (Plugin plugin in Plugins)
                {
                    BotCommand botCommand = new BotCommand();
                    botCommand.Command = plugin.PluginExecution.ExecutionCommand;
                    botCommand.Description = plugin.HelpShort;
                    botCommands.Add(botCommand);
                }
            }

            return botCommands;
        }

        public static List<Plugin> GetPLugins()
        {
            return Plugins;
        }
    }
}
