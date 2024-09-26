namespace USSDMiddleware.Core.Models.ResponseModel;

public class BalanceEnquiryResponse
{
    public string AvailableBalance { get; set; }
    public string LedgerBalance { get; set; }
    public string WithdrawableBalance { get; set; }
    public string AccountType { get; set; }
}