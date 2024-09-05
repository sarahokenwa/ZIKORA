namespace USSDMiddleware.Core.Models.Accounts
{
    public class BlockAccountRequest
    {
        public string AccountNo { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

    }
}
