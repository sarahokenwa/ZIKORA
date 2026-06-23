namespace USSDMiddleware.Core.Models
{
    public class ApiOptions
    {
        public ZikoraOptions Zikora { get; set; }
        public string CyberPayBillUrl { get; set; }
        public string CyberPayAuthUrl { get; set; }
        public string AuthUsername { get; set; }
        public string AuthPassword { get; set; }
        public string CyberPayFundTransferUrl { get; set; }
        public string PaymentUrl { get; set; }
        public string NibssCode { get; set; }
        public string GLCode { get; set; }
        public string WalletCode { get; set; }
        public decimal MerchantCharge { get; set; }
        public string WebHook {  get; set; }
        public string WalletType { get; set; }
    }

    public class ZikoraOptions
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string WalletId { get; set; }
        public string RequestType { get; set; }
        public string BIN { get; set; }
        public string DeliveryOption { get; set; }
        public string Identifier { get; set; }
        public string AccountOfficerCode { get; set; }
        public string ProductCode { get; set; }
        public string BankCode { get; set; }
        public string FTGLCode { get; set; }
        public string BillsGLCode { get; set; }
        public string NibssCode { get; set; }
        public decimal DailyLimitInKobo { get; set; }
        public decimal TransactionLimitInKobo { get; set; }
        public List<ChargeConfig> Charges { get; set; }
        public int OTPValidityInMinutes { get; set; } = 10;
        public string SMSProviderUrl { get; set; }
        public string SMSEndpoint { get; set; }
        public string SMSApiKey { get; set; } = default!;
        public string SMSSenderId { get; set; } = default!;
        public string SMSMessageTemplate { get; set; } = default!;
        public string SMSType { get; set; } = default!;
        public string Channel { get; set; } = default!;
        public string ResetInstruction { get; set; } = default!;
    }

    public class ChargeConfig
    {
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal Charge { get; set; }
    }
}
