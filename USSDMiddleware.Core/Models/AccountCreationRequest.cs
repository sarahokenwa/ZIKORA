using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Models
{
    public class AccountCreationRequest
    {
        public string TransactionTrackingRef { get; set; }
        public string AccountOpeningTrackingRef { get; set; }
        public string ProductCode { get; set; }
        public string LastName { get; set; }
        public string? OtherNames { get; set; }
        public string BVN { get; set; }
        public string AccountName { get; set; }
        public string PhoneNo { get; set; }
        public int Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string AccountOfficerCode { get; set; }
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        public string AccountTier { get; set; }

        public AccountCreationRequest(ValidationLog validationLog)
        {
            TransactionTrackingRef = "";
            AccountOpeningTrackingRef = "";
            ProductCode = "";
            LastName = validationLog.LastName;
            FirstName = validationLog.FirstName;
            OtherNames = validationLog.OtherNames;
            BVN = validationLog.Bvn;
            AccountName = $"{validationLog.LastName} {validationLog.FirstName}";
            PhoneNo = validationLog.PhoneNumber;
            Gender = 0;
            DateOfBirth = validationLog.Dob;
            AccountOfficerCode = "";
            Email = "";
            AccountTier = "";
        }
    }
}
