namespace USSDMiddleware.Core.Models.Accounts
{
    public class DeactivatePostNoDebitRequest
    {
        public string AccountNo { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
