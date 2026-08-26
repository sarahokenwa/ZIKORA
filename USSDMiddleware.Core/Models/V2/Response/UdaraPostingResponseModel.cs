namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraPostingResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraTransferData? Data { get; set; }
    }
}
