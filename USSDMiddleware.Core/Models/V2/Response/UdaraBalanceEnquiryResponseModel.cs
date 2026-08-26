namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraBalanceEnquiryResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraBalanceData? Data { get; set; }
    }

    public class UdaraBalanceData
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal LedgerBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal WithdrawableBalance { get; set; }
        public decimal LienAmount { get; set; }
    }
}
