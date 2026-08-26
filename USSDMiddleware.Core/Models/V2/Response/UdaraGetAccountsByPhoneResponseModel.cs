namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraGetAccountsByPhoneResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraAccountsData? Data { get; set; }
    }

    public class UdaraAccountsData
    {
        public List<UdaraAccountItem>? Data { get; set; }
        public int RecordCount { get; set; }
    }

    public class UdaraAccountItem
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string OtherNames { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
