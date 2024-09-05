namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class FreezeCardResponse
    {
        public bool IsSuccessful { get; set; }
        public object ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string SerialNo { get; set; }
        public string TransactionReference { get; set; }
    }
}

