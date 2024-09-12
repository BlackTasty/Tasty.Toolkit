using System.Security.Cryptography;
using System.Text;

namespace Tasty.SQLiteManager
{
    internal static class Encryption
    {
        internal static byte[] ComputeSHA256Hash(this string plain)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(plain));
            }
        }

        internal static bool CompareSHA256Hashes(this byte[] hash, string compareTo)
        {
            return hash.CompareSHA256Hashes(compareTo.ComputeSHA256Hash());
        }

        // Helper method to convert byte array to a hex string
        internal static bool CompareSHA256Hashes(this byte[] hash, byte[] compareTo)
        {
            return hash.CreateHash().Equals(compareTo.CreateHash());
        }

        private static string CreateHash(this byte[] hash)
        {
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
