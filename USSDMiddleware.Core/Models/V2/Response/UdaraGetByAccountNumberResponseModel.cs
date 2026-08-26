namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraGetByAccountNumberResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraAccountDetails? Data { get; set; }
    }

    public class UdaraAccountDetails
    {
        public string Id { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerID { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
    }
}

