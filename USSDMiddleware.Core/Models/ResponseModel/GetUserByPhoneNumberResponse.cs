namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetUserByPhoneNumberResponse
{
    public string CustomerID { get; set; }
    public string LastName { get; set; }
    public string OtherNames { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string BankVerificationNumber { get; set; }
    public string DateOfBirth { get; set; }
}