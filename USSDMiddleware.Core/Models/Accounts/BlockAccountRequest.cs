namespace USSDMiddleware.Core.Models.Accounts
{
    public class BlockAccountRequest
    {
        public string OwnersPhoneNumber { get; set; }
        public string RequestPhoneNumber { get; set; }
        public string AccountNo { get; set; }
        public string Pin{ get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

    }
}
