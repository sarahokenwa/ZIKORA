using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace USSDMiddleware.Core.Utilities
{
    public static class Utility
    {
        public static byte[] GetSalt()
        {
            byte[] salt = new byte[256];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string GenerateRandomDigits(int length)
        {
            string rand = Regex.Replace(Guid.NewGuid().ToString(), "[^1-9]", "");

            while (rand.Length < length)
            {
                rand += Regex.Replace(Guid.NewGuid().ToString(), "[^1-9]", "");
            }

            return rand.Substring(0, length);
        }
    }
}
