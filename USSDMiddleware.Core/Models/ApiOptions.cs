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
    }

}
