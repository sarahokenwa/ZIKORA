using USSDMiddleware.Core.Models.V2.Request;

namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraCreateAccountResponseModel
    {
            public bool Status { get; set; }
            public string Message { get; set; } = string.Empty;
            public UdaraCreateCustomerAccountData? Data { get; set; }
     }

        public class UdaraCreateCustomerAccountData
        {
            public string Id { get; set; } = string.Empty;
            public string AccountNumber { get; set; } = string.Empty;
            public string CustomerID { get; set; } = string.Empty;
            public string CustomerInformationId { get; set; } = string.Empty;
            public string DocumentIdentifier { get; set; } = string.Empty;
        }
}
