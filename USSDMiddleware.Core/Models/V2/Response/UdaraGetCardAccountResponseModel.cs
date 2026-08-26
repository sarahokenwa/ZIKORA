namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraGetCardAccountResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraCardAccountData? Data { get; set; }
    }

    public class UdaraCardAccountData
    {
        public List<UdaraCardAccountItem>? Data { get; set; }
        public int RecordCount { get; set; }
    }

    public class UdaraCardAccountItem
    {
        public string Id { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string NameOnCard { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public UdaraCardDetails? Card { get; set; }
    }

    public class UdaraCardDetails
    {
        public string MaskedPan { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string NameOnCard { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
