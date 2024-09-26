using System.Text;

namespace USSDMiddleware.Core.Utilities
{
    public class RandomEmailGenerator
    {
        private static readonly Random random = new Random();
        public static string GenerateRandomEmail(int length = 10)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder localPart = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                localPart.Append(chars[random.Next(chars.Length)]);
            }

            string domain = "cyberpay.net.ng";  
            return $"{localPart}@{domain}";
        }
    }
}
