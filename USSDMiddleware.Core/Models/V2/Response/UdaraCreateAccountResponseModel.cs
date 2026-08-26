using USSDMiddleware.Core.Models.V2.Request;

namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraCreateAccountResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraAccountData? Data { get; set; }
    }

    public class UdaraAccountData
    {
        public string Id { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
