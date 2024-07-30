namespace USSDMiddleware.Core.Models.Bills
{
    public class VendRequest
    {
        public string paymentCode { get; set; }
        public decimal amount { get; set; }
        public string customerId { get; set; }
        public string businessWalletId { get; set; }
        public string customerWalletId { get; set; }
        public string customerMobile { get; set; }
        public string customerEmail { get; set; }
        public string merchantRef { get; set; }
        public int paymentOptionId { get; set; }
        public string webHook { get; set; }
        public string returnUrl { get; set; }
        public string unitAmount { get; set; }
        public string quantity { get; set; }
        public string name { get; set; }
        public string address { get; set; }

        public string validationReference { get; set; }
    }
}
