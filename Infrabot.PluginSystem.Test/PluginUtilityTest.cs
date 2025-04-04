using Infrabot.PluginSystem.Utils;
using Infrabot.PluginSystem.Execution;
using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Data;
using System.Text;

namespace Infrabot.PluginSystem.Test
{
    public class PluginUtilityTest
    {
        [SetUp]
        public void Setup()
        {
        }

        #region Save Plugin

        [Test]
        public async Task SavePlugin_WritesFileAndComputesChecksum()
        {
            // Arrange
            var plugin = new Plugin
            {
                Guid = Guid.Empty,
                Id = "wOqhFnbx00",
                Name = "Test1",
                PluginType = PluginType.Administration,
                Description = "Test1 plugin for test purposes",
                Author = "Test",
                Version = 0,
                WebSite = "aaa.com",
                PluginExecutions = new List<PluginExecution>
                {
                    new PluginExecution()
                    {
                        CommandName = "/test1",
                        Help = "Write /test1 to get output",
                        ExecutionFilePath = "test1.ps1",
                        ExecutionTimeout = 10,
                        DefaultErrorMessage = "Default error",
                        ExecuteType = CommandExecuteTypes.PSScript
                    } 
                },
                PluginFiles = null,
                Settings = null
            };

            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Act
                var result = await PluginUtility.SavePlugin(plugin, tempFilePath);

                // Assert method returns true
                Assert.That(result, Is.True);

                // Assert file exists
                Assert.That(File.Exists(tempFilePath), Is.True);

                // Assert file is not empty
                var fileBytes = await File.ReadAllBytesAsync(tempFilePath);
                Assert.That(fileBytes.Length, Is.GreaterThan(0));

                // Assert Checksum is not null or empty
                Assert.That(plugin.Checksum, Is.EqualTo("fb0d86099b67be32889bd9f8a1bcc8e9296af96c9867c689ed2690410813c4f8"));

                // Optional: Deserialize and check content
                using var stream = new MemoryStream(fileBytes);
                var deserialized = ProtoBuf.Serializer.Deserialize<Plugin>(stream);

                Assert.That(deserialized.Id, Is.EqualTo(plugin.Id));
                Assert.That(deserialized.Name, Is.EqualTo(plugin.Name));
                Assert.That(deserialized.Checksum, Is.EqualTo(plugin.Checksum));
            }
            finally
            {
                File.Delete(tempFilePath); // Clean up
            }
        }

        #endregion

        #region Get Plugin

        [Test]
        public async Task GetPlugin_ReturnsPlugin_WhenFileIsValid()
        {
            // Arrange
            var plugin = new Plugin
            {
                Guid = Guid.Empty,
                Id = "wOqhFnbx00",
                Name = "Test1",
                PluginType = PluginType.Administration,
                Description = "Test1 plugin for test purposes",
                Author = "Test",
                Version = 0,
                WebSite = "aaa.com",
                PluginExecutions = new List<PluginExecution>
                {
                    new PluginExecution()
                    {
                        CommandName = "/test1",
                        Help = "Write /test1 to get output",
                        ExecutionFilePath = "test1.ps1",
                        ExecutionTimeout = 10,
                        DefaultErrorMessage = "Default error",
                        ExecuteType = CommandExecuteTypes.PSScript
                    }
                },
                PluginFiles = null,
                Settings = null
            };

            var filePath = Path.GetTempFileName();

            try
            {
                // Save valid plugin first
                await PluginUtility.SavePlugin(plugin, filePath);

                // Act
                var loaded = await PluginUtility.GetPlugin(filePath);

                // Assert
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded.Id, Is.EqualTo(plugin.Id));
                Assert.That(loaded.Name, Is.EqualTo(plugin.Name));
                Assert.That(loaded.Checksum, Is.EqualTo(plugin.Checksum));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void GetPlugin_Throws_WhenFileDoesNotExist()
        {
            // Arrange
            string invalidPath = "nonexistent.plug";

            // Act
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await PluginUtility.GetPlugin(invalidPath);
            });

            // Assert
            Assert.That(ex.Message, Does.Contain("does not exist"));
        }

        [Test]
        public async Task GetPlugin_Throws_WhenChecksumIsInvalid()
        {
            // Arrange
            var plugin = new Plugin
            {
                Guid = Guid.Empty,
                Id = "wOqhFnbx00",
                Name = "Test1",
                PluginType = PluginType.Administration,
                Description = "Test1 plugin for test purposes",
                Author = "Test",
                Version = 0,
                WebSite = "aaa.com",
                PluginExecutions = new List<PluginExecution>
                {
                    new PluginExecution()
                    {
                        CommandName = "/test1",
                        Help = "Write /test1 to get output",
                        ExecutionFilePath = "test1.ps1",
                        ExecutionTimeout = 10,
                        DefaultErrorMessage = "Default error",
                        ExecuteType = CommandExecuteTypes.PSScript
                    }
                },
                PluginFiles = null,
                Settings = null
            };

            var filePath = Path.GetTempFileName();

            try
            {
                await PluginUtility.SavePlugin(plugin, filePath);

                // Tamper the file: overwrite some bytes
                byte[] bytes = await File.ReadAllBytesAsync(filePath);
                bytes[10] = (byte)(bytes[10] + 1); // Flip a byte
                await File.WriteAllBytesAsync(filePath, bytes);

                // Act
                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await PluginUtility.GetPlugin(filePath);
                });

                // Assert
                Assert.That(ex.Message, Does.Contain("checksum mismatch"));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region Extract Plugin Files

        [Test]
        public async Task ExtractPluginFiles_CreatesFilesSuccessfully()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var plugin = new Plugin
            {
                PluginFiles = new List<PluginFile>
                {
                    new PluginFile
                    {
                        FileName = "hello.txt",
                        FilePath = "subfolder",
                        FileData = Encoding.UTF8.GetBytes("Hello world!")
                    },
                    new PluginFile
                    {
                        FileName = "root.txt",
                        FilePath = "",
                        FileData = Encoding.UTF8.GetBytes("Root file")
                    }
                }
            };

            try
            {
                // Act
                await PluginUtility.ExtractPluginFiles(plugin, tempDir);

                // Assert
                string full1 = Path.Combine(tempDir, "subfolder", "hello.txt");
                string full2 = Path.Combine(tempDir, "root.txt");

                Assert.That(File.Exists(full1), Is.True);
                Assert.That(File.ReadAllText(full1), Is.EqualTo("Hello world!"));

                Assert.That(File.Exists(full2), Is.True);
                Assert.That(File.ReadAllText(full2), Is.EqualTo("Root file"));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Test]
        public void ExtractPluginFiles_Throws_WhenPathIsInvalid()
        {
            // Arrange
            var plugin = new Plugin
            {
                PluginFiles = new List<PluginFile>
                {
                    new PluginFile
                    {
                        FileName = "test.txt",
                        FilePath = "",
                        FileData = new byte[] { 1, 2, 3 }
                    }
                }
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await PluginUtility.ExtractPluginFiles(plugin, "");
            });

            Assert.That(ex.ParamName, Is.EqualTo("destinationPath"));
        }

        [Test]
        public async Task ExtractPluginFiles_DoesNothing_WhenPluginFilesIsEmpty()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var plugin = new Plugin
            {
                PluginFiles = new List<PluginFile>()
            };

            try
            {
                // Act
                await PluginUtility.ExtractPluginFiles(plugin, tempDir);

                // Assert
                var files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
                Assert.That(files.Length, Is.EqualTo(0));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        #endregion


        #region Import Plugin Files

        [Test]
        public async Task ImportPluginFiles_ReturnsPluginFiles_WithCorrectContent()
        {
            // Arrange
            var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(baseDir);

            var subDir = Path.Combine(baseDir, "sub");
            Directory.CreateDirectory(subDir);

            var file1 = Path.Combine(baseDir, "file1.txt");
            var file2 = Path.Combine(subDir, "file2.txt");

            File.WriteAllText(file1, "Hello Plugin");
            File.WriteAllText(file2, "Inside Subfolder");

            try
            {
                // Act
                var result = await PluginUtility.ImportPluginFiles(baseDir); // Adjust class name

                // Assert
                Assert.That(result.Count, Is.EqualTo(2));

                var file1Data = result.FirstOrDefault(f => f.FileName == "file1.txt");
                var file2Data = result.FirstOrDefault(f => f.FileName == "file2.txt");

                Assert.That(file1Data, Is.Not.Null);
                Assert.That(file2Data, Is.Not.Null);

                Assert.That(Encoding.UTF8.GetString(file1Data.FileData), Is.EqualTo("Hello Plugin"));
                Assert.That(Encoding.UTF8.GetString(file2Data.FileData), Is.EqualTo("Inside Subfolder"));

                Assert.That(file1Data.FilePath, Is.EqualTo("."));
                Assert.That(file2Data.FilePath, Is.EqualTo("sub"));

                // Verify SHA256 hash is accurate
                var expectedHash = HashUtility.CalculateSHA256(file1); // uses file path
                Assert.That(file1Data.FileHash, Is.EqualTo(expectedHash));
            }
            finally
            {
                Directory.Delete(baseDir, true);
            }
        }

        [Test]
        public void ImportPluginFiles_Throws_WhenPathIsInvalid()
        {
            // Arrange
            var invalidPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await PluginUtility.ImportPluginFiles(invalidPath); // Class name
            });

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("sourcePath"));
        }

        #endregion

        #region Generate Unique PluginId

        [Test]
        public void GenerateUniquePluginId_ReturnsValidUniqueId()
        {
            // Act
            var pluginId = PluginUtility.GenerateUniquePluginId(); // Replace with actual class name

            // Assert basic format
            Assert.That(pluginId, Is.Not.Null.And.Not.Empty);
            Assert.That(pluginId.Length, Is.EqualTo(10));

            // Assert contains only alphanumeric characters (base64 without +, /, =)
            Assert.That(pluginId, Does.Match("^[a-zA-Z0-9]+$"));
        }

        [Test]
        public void GenerateUniquePluginId_IsUniqueAcrossMultipleCalls()
        {
            // Act
            var ids = Enumerable.Range(0, 1000)
                                .Select(_ => PluginUtility.GenerateUniquePluginId())
                                .ToList();

            // Assert all are 10-char strings
            Assert.That(ids.All(id => id.Length == 10), Is.True);

            // Assert uniqueness
            var unique = ids.Distinct().Count();
            Assert.That(unique, Is.EqualTo(ids.Count));
        }

        [Test]
        public void GenerateUniquePluginId_DoesNotContainSpecialCharacters()
        {
            var id = PluginUtility.GenerateUniquePluginId();
            Assert.That(id.Contains("+"), Is.False);
            Assert.That(id.Contains("/"), Is.False);
            Assert.That(id.Contains("="), Is.False);
        }


        #endregion

        #region Compute SHA256

        [Test]
        public void ComputeSHA256_ReturnsExpectedHash()
        {
            // Arrange
            var input = Encoding.UTF8.GetBytes("hello world");

            // Expected SHA256 hash of "hello world"
            const string expectedHash = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";

            // Act
            var actualHash = PluginUtility.ComputeSHA256(input); // Replace with actual class name

            // Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }

        #endregion
    }
}