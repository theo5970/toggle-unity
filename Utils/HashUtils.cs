using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Toggle.Utils
{
    public static class HashUtils
    {
        private static StringBuilder hexBuilder = new StringBuilder();
        private static SHA256 sha256 = SHA256.Create();
        public static string GenerateSHA256(string text)
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return ByteArrayToHexString(hash);
        }

        public static string GenerateSHA256(Stream stream)
        {
            byte[] hash = sha256.ComputeHash(stream);
            return ByteArrayToHexString(hash);
        }

        private static string ByteArrayToHexString(byte[] hash)
        {
            hexBuilder.Clear();
            for (int i = 0; i < hash.Length; i++)
            {
                hexBuilder.Append(hash[i].ToString("x2"));
            }

            return hexBuilder.ToString();

        }
    }
}