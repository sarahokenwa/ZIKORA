namespace USSDMiddleware.Core.Models
{
    public class ApiOptions
    {
        public ZikoraOptions Zikora { get; set; }
        public string CyberPayBillUrl { get; set; }
        public string CyberPayAuthUrl { get; set; }
        public string AuthUsername { get; set; }
        public string AuthPassword { get; set; }
    }

    public class ZikoraOptions
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string WalletId { get; set; }
    }

}
