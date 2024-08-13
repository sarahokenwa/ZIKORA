namespace USSDMiddleware.Core.Models.ResponseModel;

public class DebitCustomerAccountResponse
{
    public string Code { get; set; }
    public string? Message { get; set; }
    public bool Succeeded { get; set; }
    public ResponseData Data { get; set; }

    public class ResponseData
    {
        public bool IsSuccessful { get; set; }
        public string ResponseMessage { get; set; }
        public string ResponseCode { get; set; }
        public string Reference { get; set; }
    }
    //public bool IsSuccessful { get; set; }
    //public string ResponseMessage { get; set; }
    //public string ResponseCode { get; set; }
    //public string Reference { get; set; }
}
