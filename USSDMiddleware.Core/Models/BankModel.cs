namespace USSDMiddleware.Core.Models
{
    public class BankModel
    {
        public long Id { get; set; }
        public string BankCode { get; set; }
        public string NipCode { get; set; }
        public string BankName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDirectDebit { get; set; }
        public ICollection<BankProviderModel> BankProviders { get; set; } = new HashSet<BankProviderModel>();

        public long? ProviderCode { get; set; }

        public string ProcessingType { get; set; }
        public string ExternalRedirectUrl { get; set; }

    }

    public enum enumProcessingType
    {
        Internal,
        External
    }
}
