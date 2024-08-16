using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Models.Request
{
    public class CardRequest
    {
        public string AccountNumber { get; set; }
        public Enums.Providers Provider { get; set; }
    }
}
