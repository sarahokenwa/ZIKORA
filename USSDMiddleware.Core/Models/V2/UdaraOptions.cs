namespace USSDMiddleware.Core.Models.V2
{
    public class UdaraOptions
    {
        public const string SectionName = "Udara";

        public string BaseUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = "/api/auth/v1/token";
        public string DefaultBranchCode { get; set; } = string.Empty;
        public string DefaultAccountOfficerStaffId { get; set; } = string.Empty;
        public string DefaultProductCode { get; set; } = string.Empty;
        public int DefaultAccountTier { get; set; }
        public int DefaultAccessLevel { get; set; }
        public int DefaultAccountType { get; set; }
        public int DefaultAccountStatus { get; set; }
        public int DefaultStatementDeliveryMode { get; set; }
        public int DefaultStatementDeliveryFrequency { get; set; }
        public decimal DefaultMinimumBalanceRequired { get; set; }
        public int DefaultCategoryOfAccount { get; set; }
        public string DefaultSectorCode { get; set; } = string.Empty;
        public string DefaultFeeIncomeGL { get; set; } 

    }
}
