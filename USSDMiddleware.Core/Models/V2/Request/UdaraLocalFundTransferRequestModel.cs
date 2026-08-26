
namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraLocalFundTransferRequestModel
    {
        public string DebitAccount { get; set; } = string.Empty;
        public string CreditAccount { get; set; } = string.Empty;
        public long Amount { get; set; }                    
        public long FeeCharge { get; set; }                 
        public string? FeeIncomeGL { get; set; }
        public string? InstrumentNumber { get; set; }
        public string Narration { get; set; } = string.Empty;
        public int PostingsTransactionType { get; set; } = 3; 
    }
}
