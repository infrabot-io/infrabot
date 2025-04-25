using Infrabot.PluginSystem.Data;
using Infrabot.PluginSystem.Execution;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Infrabot.PluginSystem.Utils
{
    public class PluginUtility
    {
        public static async Task<bool> SavePlugin(Plugin plugin, string filePath)
        {
            if (plugin.Settings is not null)
            {
                if (plugin.Settings.Count > 0)
                {
                    foreach (PluginSetting pluginSetting in plugin.Settings)
                    {
                        pluginSetting.Value = EncryptionUtility.TripleDesEncrypt(plugin.Guid.ToString(), pluginSetting.Value);
                    }
                }
            }

            plugin.Checksum = null;
            using var memoryStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(memoryStream, plugin);
            byte[] pluginData = memoryStream.ToArray();

            plugin.Checksum = ComputeSHA256(pluginData);
            using var finalStream = new MemoryStream();

            ProtoBuf.Serializer.Serialize(finalStream, plugin);
            await File.WriteAllBytesAsync(filePath, finalStream.ToArray());

            return true;
        }

        public static async Task<Plugin> GetPlugin(string pluginFilePath)
        {
            if (!File.Exists(pluginFilePath))
                throw new InvalidOperationException("Plugin file does not exist.");

            byte[] fileData = await File.ReadAllBytesAsync(pluginFilePath);
            using var memoryStream = new MemoryStream(fileData);
            var plugin = ProtoBuf.Serializer.Deserialize<Plugin>(memoryStream);

            if (plugin.Settings is not null)
            {
                if (plugin.Settings.Count > 0)
                {
                    foreach (PluginSetting pluginSetting in plugin.Settings)
                    {
                        pluginSetting.Value = EncryptionUtility.TripleDesDecrypt(plugin.Guid.ToString(), pluginSetting.Value);
                    }
                }
            }

            memoryStream.Position = 0;
            var tempPlugin = ProtoBuf.Serializer.Deserialize<Plugin>(memoryStream);
            tempPlugin.Checksum = null;

            using var tempStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(tempStream, tempPlugin);
            string computedChecksum = ComputeSHA256(tempStream.ToArray());


            if (plugin.Checksum != computedChecksum)
                throw new InvalidOperationException("Plugin checksum mismatch.");

            return plugin;
        }

        public static Plugin DescryptPluginSecrets(Plugin plugin)
        {
            if (plugin.Settings is not null)
            {
                if (plugin.Settings.Count > 0)
                {
                    foreach (PluginSetting pluginSetting in plugin.Settings)
                    {
                        pluginSetting.Value = EncryptionUtility.TripleDesDecrypt(plugin.Guid.ToString(), pluginSetting.Value);
                    }
                }
            }

            return plugin;
        }

        public static Plugin GetPlugin(Stream pluginStream)
        {
            byte[] fileData;
            using (var memoryStream2 = new MemoryStream())
            {
                pluginStream.CopyTo(memoryStream2);
                fileData = memoryStream2.ToArray();
            }

            using var memoryStream = new MemoryStream(fileData);
            var plugin = ProtoBuf.Serializer.Deserialize<Plugin>(memoryStream);

            memoryStream.Position = 0;
            var tempPlugin = ProtoBuf.Serializer.Deserialize<Plugin>(memoryStream);
            tempPlugin.Checksum = null;

            using var tempStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(tempStream, tempPlugin);
            string computedChecksum = ComputeSHA256(tempStream.ToArray());

            if (plugin.Checksum != computedChecksum)
                throw new InvalidOperationException("Plugin checksum mismatch.");

            if (plugin.Settings is not null)
            {
                if (plugin.Settings.Count > 0)
                {
                    foreach (PluginSetting pluginSetting in plugin.Settings)
                    {
                        Console.WriteLine("Before: "+ pluginSetting.Value);
                        pluginSetting.Value = EncryptionUtility.TripleDesDecrypt(plugin.Guid.ToString(), pluginSetting.Value);
                        Console.WriteLine("After: " + pluginSetting.Value);
                    }
                }
            }

            return plugin;
        }

        public static async Task ExtractPluginFiles(Plugin plugin, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationPath));

            destinationPath = destinationPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            // No files to extract
            if (plugin?.PluginFiles == null || plugin.PluginFiles.Count == 0)
                return; 

            foreach (PluginFile pluginFile in plugin.PluginFiles)
            {
                var filePath = Path.Combine(destinationPath, pluginFile.FilePath ?? string.Empty);
                var fullFilePath = Path.Combine(filePath, pluginFile.FileName);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        SetDirectoryPermissions(filePath);
                    }
                }

                // Write file
                await File.WriteAllBytesAsync(fullFilePath, pluginFile.FileData);

                // Set permission
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    File.SetUnixFileMode(fullFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                          UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                                          UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                }
            }
        }

        public static async Task<List<PluginFile>> ImportPluginFiles(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
                throw new ArgumentException("Source path is invalid or does not exist.", nameof(sourcePath));

            List<PluginFile> pluginFiles = new List<PluginFile>();

            var directoryFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

            foreach (var filePath in directoryFiles)
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(sourcePath, fileInfo.DirectoryName ?? string.Empty);

                pluginFiles.Add(new PluginFile
                {
                    FileName = fileInfo.Name,
                    FilePath = relativePath,
                    FileHash = HashUtility.CalculateSHA256(fileInfo.FullName),
                    FileData = await File.ReadAllBytesAsync(filePath)
                });
            }

            return pluginFiles;
        }

        public static string GenerateUniquePluginId()
        {
            string guidString = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) // Convert GUID to Base64
                                .Replace("=", "")  // Remove padding
                                .Replace("+", "")  // Remove special characters
                                .Replace("/", ""); // Remove special characters

            return guidString[..10]; // Return first 10 characters
        }

        public static string ComputeSHA256(byte[] data)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static void SetDirectoryPermissions(string directoryPath)
        {
            var chmod = new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"755 \"{directoryPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(chmod);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                Console.WriteLine($"Failed to set permissions: {error}");
            }
            else
            {
                Console.WriteLine("Directory permissions set to 755.");
            }
        }
    }
}
