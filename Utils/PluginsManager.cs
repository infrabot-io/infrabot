using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfraBot.Core
{
    public class PluginsManager
    {
        public static FileSystemWatcher watcher = new FileSystemWatcher();
        public static string PluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";

        public PluginsManager()
        {
            // Check if plugins folder exist, and if not create it
            if (Directory.Exists(PluginsPath) == false)
            {
                Directory.CreateDirectory(PluginsPath);
            }

            // Watch our plugins path for changes
            watcher.Path = PluginsPath;

            // Trigger on changes
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            // React only for .plug file extension
            watcher.Filter = "*.plug";

            // Perform actions when event happened
            watcher.Changed += FileOnChanged;
            watcher.Created += FileOnCreated;
            watcher.Deleted += FileOnDeleted;
            watcher.Renamed += FileOnRenamed;

            // Enable our FileSystemWatcher
            watcher.EnableRaisingEvents = true;
        }

        private static void FileOnChanged(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;

            try
            {
                // We have to set "watcher.EnableRaisingEvents = false" because on each event
                // this part may execute many times. To avoid this behaviour we temporarily 
                // turn it off, and then turn it on again at the end

                watcher.EnableRaisingEvents = false;

                // Delete our old plugin folder path since our plugin file changed
                if (Directory.Exists(FolderPath))
                {
                    Directory.Delete(FolderPath, true);
                    WaitForDeletion(FolderPath);
                }

                // Recreate new folder with new name
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                // Extract contents of our plugin
                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnChanged. Could not change plugin in folder: {FolderPath}. Error was: {ex.Message}");
            }
            finally
            {
                CommandCenter.config.telegram_commands = LoadPlugins();
                WriteToPluginsLog($"FileOnChanged. Reload plugins. Found plugins count: " + CommandCenter.config.telegram_commands.Count.ToString());

                // Turn back on event triggering
                watcher.EnableRaisingEvents = true;
            }
        }

        private static void FileOnCreated(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;

            // Delete if folder with our new plugin name exists
            try
            {
                if (Directory.Exists(FolderPath))
                {
                    Directory.Delete(FolderPath, true);
                    WaitForDeletion(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnCreated. Could not delete folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnCreated. Can not deploy plugin!");
                WriteToPluginsLog($"FileOnCreated. Try to delete folder manually!");
                return;
            }

            // Create folder with the same name like our plugin
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnCreated. Could not create folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnCreated. Can not deploy plugin!");
                return;
            }

            // Extract our plugin contents into newly created folder
            try
            {
                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnCreated. Could not extract plugin to folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnCreated. Can not deploy plugin!");
            }

            CommandCenter.config.telegram_commands = LoadPlugins();
            WriteToPluginsLog($"FileOnCreated. Reload plugins. Found plugins count: " + CommandCenter.config.telegram_commands.Count.ToString());
        }

        private static void FileOnDeleted(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;

            // Just delete plugin folder
            try
            {
                if (Directory.Exists(FolderPath))
                {
                    Directory.Delete(FolderPath, true);
                    WaitForDeletion(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnDeleted. Could not delete folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnDeleted. Can not delete plugin!");
                WriteToPluginsLog($"FileOnDeleted. Try to delete folder manually");
                return;
            }

            CommandCenter.config.telegram_commands = LoadPlugins();
            WriteToPluginsLog($"FileOnDeleted. Reload plugins. Found plugins count: " + CommandCenter.config.telegram_commands.Count.ToString());
        }

        private static void FileOnRenamed(object source, RenamedEventArgs e)
        {
            string OldFolderName = e.OldName.Replace(".plug", "");
            string OldFolderPath = PluginsPath + @"\" + OldFolderName;
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;

            // Delete old plugin folder
            try
            {
                if (Directory.Exists(OldFolderPath))
                {
                    Directory.Delete(OldFolderPath, true);
                    WaitForDeletion(OldFolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnRenamed. Could not delete folder: {OldFolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnRenamed. Can not redeploy plugin with new name!");
                WriteToPluginsLog($"FileOnRenamed. Try to delete folder manually");
                return;
            }

            // Create new folder with new plugin name where plugin will be redeployed
            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnRenamed. Could not create folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnRenamed. Can not redeploy plugin with new name!");
                return;
            }

            // Extract contents of plugin into new folder
            try
            {
                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToPluginsLog($"FileOnRenamed. Could not extract plugin to folder: {FolderPath}. Error was: {ex.Message}");
                WriteToPluginsLog($"FileOnRenamed. Can not redeploy plugin with new name!");
            }

            CommandCenter.config.telegram_commands = LoadPlugins();
            WriteToPluginsLog($"FileOnRenamed. Reload plugins. Found plugins count: " + CommandCenter.config.telegram_commands.Count.ToString());
        }

        private static void WaitForDeletion(string directoryName)
        {
            bool deleted = false;
            do
            {
                deleted = !System.IO.Directory.Exists(directoryName);
                DateTime now = DateTime.Now;
                System.Threading.Thread.Sleep(100);
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
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"logs\plugins.log", localDate.ToString() + ": " + Log + Environment.NewLine);
            }
            catch { }
        }

        public static List<Command> LoadPlugins()
        {
            List<Command> commands = new List<Command>();

            string[] PluginsDirectories = Directory.GetDirectories(PluginsPath);
            foreach (string PluginDirectory in PluginsDirectories)
            {
                try
                {
                    if (File.Exists(PluginDirectory + @"\plugin.json"))
                    {
                        string PluginJson = File.ReadAllText(PluginDirectory + @"\plugin.json");
                        Command cm = JsonConvert.DeserializeObject<Command>(PluginJson);
                        cm.command_execute_file = PluginDirectory + @"\" + cm.command_execute_file;
                        commands.Add(cm);
                        WriteToPluginsLog($"Reloaded plugin from: " + PluginDirectory);
                    }
                }
                catch
                {
                    WriteToPluginsLog($"Can not load plugin from: " + PluginDirectory);
                }
            }

            return commands;
        }
    }
}