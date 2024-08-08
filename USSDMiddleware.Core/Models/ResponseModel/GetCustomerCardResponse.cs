namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetCustomerCardResponse
{
    public bool isSuccessful { get; set; }
    public string ResponseDescription { get; set; }
    public List<CardModel> Cards { get; set; }
}