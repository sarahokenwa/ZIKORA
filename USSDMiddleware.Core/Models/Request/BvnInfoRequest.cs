namespace USSDMiddleware.Core.Models.Request;

using USSDMiddleware.Core.Enums;
public class BvnInfoRequest
{
    public string Bvn { get; set; }
    public string PhoneNumber { get; set; }
    public Providers Provider { get; set; } = Providers.ZIKORA;
}