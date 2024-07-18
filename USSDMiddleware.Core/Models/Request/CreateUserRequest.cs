namespace USSDMiddleware.Core.Models.Request;

public class CreateUserRequest
{
    public string PhoneNumber { get; set; }
    public Enums.Providers Provider { get; set; }
}