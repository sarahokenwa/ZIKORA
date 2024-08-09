namespace USSDMiddleware.Core.Models.PayOut
{
    public class InstantPayOutRequest
    {
        public string WalletCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string SenderName { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string WebHook { get; set; }
        public string MerchantRef { get; set; }
        public string Narration { get; set; }
        public string WalletType { get; set; }
        public int MerchantCharge { get; set; }
    }
}
