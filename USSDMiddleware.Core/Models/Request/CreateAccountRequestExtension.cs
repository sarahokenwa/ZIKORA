namespace USSDMiddleware.Core.Models.Request
{
    public class CreateAccountRequestExtension : CreateAccountRequest
    {
        public string? Email { get; set; }
        public string? AccountOfficerCode { get; set; }
        public string? ProductCode { get; set; }
    }
}
