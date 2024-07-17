namespace USSDMiddleware.Core.Models.ResponseModel;

public class DebitCustomerAccountResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseMessage { get; set; }
    public string ResponseCode { get; set; }
    public string Reference { get; set; }
}