namespace USSDMiddleware.Core.Models.ResponseModel;

public class CreateAccountResponse
{
    public string? reference { get; set; }
    public string? phoneNumber { get; set; }
    public string? userId { get; set; }
    public string? Message { get; set; }

    public CreateAccountResponse(string reference, string phoneNumber, string userId, string? Message = null)
    {
        this.reference = reference;
        this.phoneNumber = phoneNumber;
        this.userId = userId;
        this.Message = Message;
    }

    public CreateAccountResponse() { }

}