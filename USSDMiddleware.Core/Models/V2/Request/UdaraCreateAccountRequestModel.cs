namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraCreateAccountRequestModel
    {
        public class UdaraCreateCustomerAccountRequest
        {
            public string? ReferenceNumber { get; set; }
            public string LastName { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string? OtherNames { get; set; }
            public string AccountName { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
            public string ProductCode { get; set; } = string.Empty;
            public string AccountOfficerStaffID { get; set; } = string.Empty;
            public string? Bvn { get; set; }
            public int? Gender { get; set; }                 // Female=1, Male=2
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? DateOfBirth { get; set; }          // YYYY-MM-DD
            public int AccountTierLevel { get; set; }
            public int AccessLevel { get; set; }
            public int AccountType { get; set; }
            public int AccountStatus { get; set; }
            public bool EnableEmailNotification { get; set; } = true;
            public bool EnableSMSNotification { get; set; } = true;
            public decimal MinimumBalanceRequired { get; set; }
            public int CategoryOfAccount { get; set; }
            public string SectorCode { get; set; } = string.Empty;
            public bool IsMinor { get; set; }
        }
    }
}

