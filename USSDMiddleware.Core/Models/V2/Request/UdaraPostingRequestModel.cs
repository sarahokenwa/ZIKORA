namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraPostingRequestModel
    {
        public List<UdaraPostingEntry> PostingEntryRequest { get; set; } = new();
        public UdaraPostingData PostingDataRequest { get; set; } = new();
    }

    public class UdaraPostingEntry
    {
        public string AccountNumber { get; set; } = string.Empty;
        public long Amount { get; set; }                 
        public int RecordType { get; set; }               // 1 = Debit, 2 = Credit
        public string Narration { get; set; } = string.Empty;
        public string? InstrumentNumber { get; set; }
    }

    public class UdaraPostingData
    {
        public bool UnplaceLienAfterPosting { get; set; } = false;
        public string LienReferenceNumber { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
    }
}
