namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraCreateAccountRequestModel
    {
        public string CustomerID { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public int AccountType { get; set; }
        public int AccountStatus { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string AccountOfficerStaffID { get; set; } = string.Empty;
        public int AccountTierLevel { get; set; }
        public int AccessLevel { get; set; }
        public bool EnableEmailNotification { get; set; } = true;
        public bool EnableSMSNotification { get; set; } = true;
        public int StatementDeliveryMode { get; set; }
        public int StatementDeliveryFrequency { get; set; }
        public decimal MinimumBalanceRequired { get; set; }
        public int CategoryOfAccount { get; set; }
        public string SectorCode { get; set; } = string.Empty;
        public string GroupConnectionID { get; set; } = string.Empty;
        public bool IsMinor { get; set; }
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianPhoneNumber { get; set; } = string.Empty;
        public string GuardianAddress { get; set; } = string.Empty;
        public string GuardianBVN { get; set; } = string.Empty;
        public string GuardianNIN { get; set; } = string.Empty;
        public UdaraRefereeInformation? RefereeInformation { get; set; }
    }

        public class UdaraRefereeInformation
        {
            public string Referee1CustomerID { get; set; } = string.Empty;
            public string Referee2CustomerID { get; set; } = string.Empty;
        }
    }

