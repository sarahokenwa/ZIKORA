namespace USSDMiddleware.Core.Models.Request;

using USSDMiddleware.Core.Enums;

public class PhoneValidationRequest
{
    public string PhoneNumber { get; set; }
    public Providers Provider { get; set; } = Providers.ZIKORA; 
}