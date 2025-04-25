using System.Security.Cryptography;
using System.Text;

namespace Infrabot.PluginSystem.Utils
{
    public static class HashUtility
    {
        public static string CalculateSHA256(string filename)
        {
            if (!File.Exists(filename))
                return "file_does_not_exist";

            using (FileStream stream = File.OpenRead(filename))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    StringBuilder hash = new StringBuilder();
                    byte[] checksum = sha256.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
        }
    }
}
