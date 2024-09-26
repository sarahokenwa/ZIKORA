namespace USSDMiddleware.Core.Models
{
    public class BankProviderModel
    {
        public long Id { get; set; }
        public string BankCode { get; set; }
        public BankModel Bank { get; set; }

        public long ProviderId { get; set; }
        public ProviderModel provider { get; set; }
        public bool IsDefault { get; set; } = false;
        public int priority { get; set; }

        public enumProcessingType ProcessingType { get; set; }

        public string ExternalRedirectUrl { get; set; }
    }
}
