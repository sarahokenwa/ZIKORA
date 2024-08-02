namespace USSDMiddleware.Core.Models.Bills
{
    public class VendResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public VendResponseData Data { get; set; }
    }

    public class VendResponseData
    {
        public string BillerName { get; set; }
        public string PaymentItemName { get; set; }
        public string TransactionReference { get; set; }
    }
}
