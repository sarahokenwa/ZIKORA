namespace USSDMiddleware.Core.Models.ResponseModel;

public class AccountCreationResponse
{
    public string? Reference { get; set; }
    public string? CustomerId { get; set; }
    public string? AccountNumber { get; set; }
    public string? FullName { get; set; }

    public AccountCreationResponse()
    {
    }

    public AccountCreationResponse(string? reference, string? customerId, string? accountNumber, string? fullName = null)
    {
        Reference = reference;
        CustomerId = customerId;
        AccountNumber = accountNumber;
        FullName = fullName;
    }
}
