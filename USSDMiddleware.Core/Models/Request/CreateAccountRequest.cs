namespace USSDMiddleware.Core.Models.Request;

using USSDMiddleware.Core.Enums;
public class CreateAccountRequest
{
    public string ValidationReference { get; set; }
    public string TransactionPin { get; set; }
    public int Gender { get; set; }
    public Providers provider { get; set; } = Providers.ZIKORA;
}