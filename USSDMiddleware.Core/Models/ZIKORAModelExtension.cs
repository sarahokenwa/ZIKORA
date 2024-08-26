namespace USSDMiddleware.Core.Models
{
    public class ZIKORAModelExtension 
    {
        public string? GLCode { get; set; }
        public string? NibssCode { get; set;}
        public decimal? FundTransferFee { get; set; }
        public string? RetrievalReference { get; set; }
        public string? WalletCode { get; set; } 
        public string? MerchantCharge { get; set;}
        public string? WebHook { get; set; }
        //public string BIN {  get; set; }
        //public string RequestType { get; set; }
        //public string DeliveryOption { get; set; }
        //public string Identifier { get; set; }
    }
}
