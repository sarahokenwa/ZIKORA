namespace USSDMiddleware.Core.Models.Request
{
    public class FreezeCardRequest
    {
        public string? SerialNo { get; set; }
        public string? Reference { get; set; }
        public string Reason { get; set; }
        public string AccountNumber { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
