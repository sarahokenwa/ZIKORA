using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace USSDMiddleware.Core.Utilities
{
    public static class Extension
    {
        public static string HashSecret(this string secret, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: secret,
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA1,
               iterationCount: 10000,
               numBytesRequested: 256 / 8));
        }

        public static string EncryptTransactionPin(this string transactionPin, byte[] salt) 
        {
            return HashSecret(transactionPin, salt);
        }

    }
}
