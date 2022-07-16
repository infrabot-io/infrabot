using infrabot.PluginSystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace infrabot.PluginSystem.Utils
{
    public class PluginActions
    {
        public static bool SavePlugin(Plugin plugin, string filePath)
        {
            bool result = false;
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            ProtoBuf.Serializer.Serialize<Plugin>(stream, plugin);
            writer.Flush();
            stream.Position = 0;

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            stream.Close();
            result = true;

            return result;
        }

        public static Plugin GetPlugin(string pluginFilePath)
        {
            MemoryStream stream = new MemoryStream(File.ReadAllBytes(pluginFilePath));
            Plugin plugin = ProtoBuf.Serializer.Deserialize<Plugin>(stream);

            return plugin;
        }

        public static void ExtractPluginFiles(Plugin plugin, string destinationPath)
        {
            if (destinationPath.EndsWith("\\") == false)
            {
                destinationPath = destinationPath + "\\";
            }

            if(plugin.PluginFiles != null)
            {
                foreach(PluginFile pluginFile in plugin.PluginFiles)
                {
                    if(Directory.Exists(destinationPath + pluginFile.PluginFilePath) == false)
                    {
                        Directory.CreateDirectory(destinationPath + pluginFile.PluginFilePath);
                    }

                    if(File.Exists(destinationPath + pluginFile.PluginFilePath + "\\" + pluginFile.PluginFileName))
                    {
                        File.Delete(destinationPath + pluginFile.PluginFilePath + "\\" + pluginFile.PluginFileName);
                    }

                    File.WriteAllBytes(destinationPath + pluginFile.PluginFilePath + "\\" + pluginFile.PluginFileName, pluginFile.PluginFileData);
                }
            }
        }

        public static List<PluginFile> ImportPluginFiles(string sourcePath)
        {
            List<PluginFile> PluginFileList = new List<PluginFile>();

            List<String> directoryFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();

            foreach (String directoryFile in directoryFiles)
            {
                FileInfo fileInfo = new FileInfo(directoryFile);

                string absolutePath = fileInfo.Directory.ToString().Replace(sourcePath, "");
                if (absolutePath.StartsWith("\\"))
                {
                    absolutePath.Substring(1);
                }

                PluginFile pluginFile = new PluginFile
                {
                    PluginFileName = fileInfo.Name,
                    PluginFilePath = absolutePath,
                    PluginFileData = File.ReadAllBytes(fileInfo.FullName)
                };

                PluginFileList.Add(pluginFile);
            }

            return PluginFileList;
        }
    }
}
