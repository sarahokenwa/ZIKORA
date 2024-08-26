namespace USSDMiddleware.Core.Models.Request
{
    public class CardRequest
    {
        public string AccountNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string TransactionPin { get; set; }
        public string? NameOnCard { get; set; }
        public Enums.Providers Provider { get; set; }
    }
}
