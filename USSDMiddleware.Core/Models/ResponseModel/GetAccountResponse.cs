namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetAccountResponse
{
    public string AccountNumber { get; set; }
    public string AccountType { get; set; }
    public string AccountStatus { get; set; }
    public string AccessLevel { get; set; }
}