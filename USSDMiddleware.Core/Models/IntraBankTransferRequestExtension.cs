using USSDMiddleware.Core.Models.Request;

namespace USSDMiddleware.Core.Models
{
    public class IntraBankTransferRequestExtension : IntraBankTransferRequest
    {
        public string PhoneNumber { get; set; }
        public string TransactionPin { get; set; }
    }
}
