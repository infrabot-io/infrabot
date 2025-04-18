using Infrabot.PluginSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrabot.PluginSystem.Test
{
    public class HashUtilityTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CalculateSHA256_ReturnsCorrectHash_WhenFileExists()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "Hello, world!");
                // Known SHA256 hash of "Hello, world!" text
                string expectedHash = "315f5bdb76d078c43b8ac0064e4a0164612b1fce77c869345bfc94c75894edd3".ToUpperInvariant();

                // Act
                var result = HashUtility.CalculateSHA256(tempFile);

                // Assert
                Assert.That(result, Is.EqualTo(expectedHash));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Test]
        public void CalculateSHA256_ReturnsErrorString_WhenFileDoesNotExist()
        {
            // Arrange
            var invalidPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            // Act
            var result = HashUtility.CalculateSHA256(invalidPath);

            // Assert
            Assert.That(result, Is.EqualTo("file_does_not_exist"));
        }

        [Test]
        public void CalculateSHA256_ReturnsHexStringWithoutDashes()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "sample");

                var hash = HashUtility.CalculateSHA256(tempFile);

                // Assert format (length = 64 hex chars, no dashes)
                Assert.That(hash, Does.Match("^[A-F0-9]{64}$"));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
