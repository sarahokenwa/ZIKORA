using System.Security.Cryptography;

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

    }
}
