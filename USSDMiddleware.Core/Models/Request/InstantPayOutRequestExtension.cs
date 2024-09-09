using USSDMiddleware.Core.Models.PayOut;

namespace USSDMiddleware.Core.Models.Request
{
    public class InstantPayOutRequestExtension : InstantPayOutRequest
    {
        public string? PhoneNumber { get; set; }
        public string? TransactionPin { get; set; }
    }
}
