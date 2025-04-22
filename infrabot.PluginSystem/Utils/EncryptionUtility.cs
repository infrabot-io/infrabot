using System.Security.Cryptography;
using System.Text;

namespace Infrabot.PluginSystem.Utils
{
    public static class EncryptionUtility
    {
        public static string TripleDesEncrypt(string key, string text)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            byte[] keyBytes = GetFixedLengthKey(key);

            using (TripleDES tripleDES = TripleDES.Create())
            {
                tripleDES.Key = keyBytes;
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;

                using ICryptoTransform encryptor = tripleDES.CreateEncryptor();
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public static string TripleDesDecrypt(string key, string base64Text)
        {
            byte[] encryptedBytes = Convert.FromBase64String(base64Text);
            byte[] keyBytes = GetFixedLengthKey(key);

            using (TripleDES tripleDES = TripleDES.Create())
            {
                tripleDES.Key = keyBytes;
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;

                using ICryptoTransform decryptor = tripleDES.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        private static byte[] GetFixedLengthKey(string key)
        {
            // TripleDES requires a key of 16 or 24 bytes
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length < 24)
            {
                Array.Resize(ref keyBytes, 24); // pad with zeros
            }
            else if (keyBytes.Length > 24)
            {
                Array.Resize(ref keyBytes, 24); // trim to 24
            }
            return keyBytes;
        }
    }
}
