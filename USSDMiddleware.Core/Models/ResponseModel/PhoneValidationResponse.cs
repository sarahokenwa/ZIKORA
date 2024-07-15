namespace USSDMiddleware.Core.Models.ResponseModel;

public class PhoneValidationResponse
{
    public bool Status { get; set; }
    public bool CanRegister { get; set; }
    public string Message { get; set; }

    public PhoneValidationResponse()
    {
                
    }
    public PhoneValidationResponse(bool status, bool canRegister, string message)
    {
        Status = status;
        CanRegister = canRegister;
        Message = message;
    }
}