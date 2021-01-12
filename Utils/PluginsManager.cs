using System;
using System.IO;
using System.IO.Compression;

namespace InfraBot.Core
{
    public class PluginsManager
    {
        static FileSystemWatcher watcher = new FileSystemWatcher();
        static string PluginsPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";
        public PluginsManager()
        {
            if (Directory.Exists(PluginsPath) == false)
            {
                Directory.CreateDirectory(PluginsPath);
            }

            watcher.Path = PluginsPath;

            watcher.NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            watcher.Filter = "*.plug";

            watcher.Changed += FileOnChanged;
            watcher.Created += FileOnCreated;
            watcher.Deleted += FileOnDeleted;
            watcher.Renamed += FileOnRenamed;

            watcher.EnableRaisingEvents = true;
        }

        private static void FileOnChanged(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;

            try
            {
                watcher.EnableRaisingEvents = false;
                if (Directory.Exists(FolderPath))
                {
                    Directory.Delete(FolderPath, true);
                    WaitForDeletion(FolderPath);
                }

                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToLog($"FileOnChanged. Could not change plugin in folder: {FolderPath}. Error was: {ex.Message}");
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        private static void FileOnCreated(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;
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
                WriteToLog($"FileOnCreated. Could not delete folder: {FolderPath}. Error was: {ex.Message}");
                WriteToLog($"FileOnCreated. Try to delete folder manually");
            }

            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"FileOnCreated. Could not create folder: {FolderPath}. Error was: {ex.Message}");
            }

            try
            {
                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToLog($"FileOnCreated. Could not extract plugin to folder: {FolderPath}. Error was: {ex.Message}");
            }
        }

        private static void FileOnDeleted(object source, FileSystemEventArgs e)
        {
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;
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
                WriteToLog($"FileOnDeleted. Could not delete folder: {FolderPath}. Error was: {ex.Message}");
                WriteToLog($"FileOnDeleted. Try to delete folder manually");
            }
        }

        private static void FileOnRenamed(object source, RenamedEventArgs e)
        {
            string OldFolderName = e.OldName.Replace(".plug", "");
            string OldFolderPath = PluginsPath + @"\" + OldFolderName;
            string FolderName = e.Name.Replace(".plug", "");
            string FolderPath = PluginsPath + @"\" + FolderName;
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
                WriteToLog($"FileOnRenamed. Could not delete folder: {OldFolderPath}. Error was: {ex.Message}");
                WriteToLog($"FileOnRenamed. Try to delete folder manually");
            }

            try
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"FileOnRenamed. Could not create folder: {FolderPath}. Error was: {ex.Message}");
            }

            try
            {
                ZipFile.ExtractToDirectory(e.FullPath, FolderPath);
            }
            catch (Exception ex)
            {
                WriteToLog($"FileOnRenamed. Could not extract plugin to folder: {FolderPath}. Error was: {ex.Message}");
            }
        }

        static void WaitForDeletion(string directoryName)
        {
            bool deleted = false;
            do
            {
                deleted = !System.IO.Directory.Exists(directoryName);
                DateTime now = DateTime.Now;
                System.Threading.Thread.Sleep(100);
            } while (!deleted);
        }

        public static void WriteToLog(string Log)
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
    }
}