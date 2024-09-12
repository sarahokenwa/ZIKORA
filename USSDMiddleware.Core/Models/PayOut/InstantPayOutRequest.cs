namespace USSDMiddleware.Core.Models.PayOut
{
    public class InstantPayOutRequest
    {
        public string SenderName { get; set; }
        public string BeneficiaryName { get; set; }
        public string AccountNumber { get; set; }
        public string SenderAccountNumber { get; set; }
        public string BankCode { get; set; }
        public decimal Amount { get; set; }
        public string TransactionPin { get; set; }
        public string PhoneNumber { get; set; }
        public string Narration { get; set; }
        public string? MerchantRef {  get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
