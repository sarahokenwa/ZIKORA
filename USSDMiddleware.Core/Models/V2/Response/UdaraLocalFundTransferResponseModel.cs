namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraLocalFundTransferResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraTransferData? Data { get; set; }
    }

    public class UdaraTransferData
    {
        public string StatusCode { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string InstrumentNumber { get; set; } = string.Empty;
    }
}
