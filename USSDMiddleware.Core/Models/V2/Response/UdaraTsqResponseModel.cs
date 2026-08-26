namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraTsqResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraTsqData? Data { get; set; }
    }

    public class UdaraTsqData
    {
        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public string ChannelCode { get; set; } = string.Empty;
        public string SourceInstitutionCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string ProcessingStatus { get; set; } = string.Empty;
        // Pending, Processing, PendingTSQ, ConfirmManually, Processed, Reversed, Failed
    }
}
