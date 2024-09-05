namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetCustomerCardResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseDescription { get; set; }
    public Card[] Cards { get; set; }
}

public class Card
{
    public string AccountNumber { get; set; }
    public string CardPAN { get; set; }
    public DateTime LinkedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string SerialNo { get; set; }
    public string NameOnCard { get; set; }
    public string Status { get; set; }
}

