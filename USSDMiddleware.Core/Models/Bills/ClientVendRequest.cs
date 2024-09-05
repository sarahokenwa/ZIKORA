namespace USSDMiddleware.Core.Models.Bills
{
    public class ClientVendRequest
    {
        public string PaymentCode { get; set; }
        public decimal Amount { get; set; }
        public string CustomerId { get; set; }
        public string CustomerMobile { get; set; }
       // public string merchantRef { get; set; }
        public string validationReference { get; set; }
        public decimal fee { get; set; }
        public string TransactionPin { get; set; }
        public string DrAccountNumber { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
    
}
