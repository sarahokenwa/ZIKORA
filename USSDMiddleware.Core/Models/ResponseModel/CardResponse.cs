namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class CardResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public BatchIssuanceData Data { get; set; }
        //public bool IsSuccessful { get; set; }
        //public string ResponseMessage { get; set; }
        //public string BatchNo { get; set; }
        //public string Identifier { get; set; }
    }

    public class BatchIssuanceData
    {
        public bool IsSuccessful { get; set; }
        public string ResponseMessage { get; set; }
        public string BatchNo { get; set; }
        public string Identifier { get; set; }
    }
}
