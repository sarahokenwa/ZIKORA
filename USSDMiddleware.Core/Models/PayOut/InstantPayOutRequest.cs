namespace USSDMiddleware.Core.Models.PayOut
{
    public class InstantPayOutRequest
    {
        public string WalletCode { get; set; }
        public string SenderAccountNumber { get; set; }
        public string SenderAccountName { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BankCode { get; set; }
        public decimal Amount { get; set; }
        public string TransactionPin { get; set; }
        public string PhoneNumber { get; set; }
        public string Narration { get; set; }
        public string MerchantReference { get; set; }
        public decimal MerchantCharge { get; set; }
        public string Webhook { get; set; }
        public string WalletType { get; set; }
        public string ProcessorRef { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

       /* public string WalletCode { get; set; }
        public string SenderAccountNumber { get; set; }
        public string SenderAccountName { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BankCode { get; set; }
        public decimal Amount { get; set; }
        public string TransactionPin { get; set; }
        public string PhoneNumber { get; set; }
        public string Narration {  get; set; }
        public string MerchantReference { get; set; }
        public decimal MerchantCharge { get; set; }
        public string Webhook {  get; set; }
        public string WalletType { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;*/

        //public string WalletCode { get; set; }
        //public string BeneficiaryName { get; set; }
        //public string SenderName { get; set; }
        //public int Amount { get; set; }
        //public string AccountNumber { get; set; }
        //public string BankCode { get; set; }
        //public string WebHook { get; set; }
        //public string MerchantRef { get; set; }
        //public string Narration { get; set; }
        //public string WalletType { get; set; }
        //public int MerchantCharge { get; set; }
    }
}
