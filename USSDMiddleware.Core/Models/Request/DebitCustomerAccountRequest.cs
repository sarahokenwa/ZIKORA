namespace USSDMiddleware.Core.Models.Request;

public class DebitCustomerAccountRequest
{
    public string RetrievalReference { get; set; }
    
    public string AccountNumber { get; set; }
    public string NibssCode { get; set; }

    public decimal Amount { get; set; }

    public decimal Fee { get; set; }

    public string Narration { get; set; }

    public string GLCode { get; set; }
}