namespace USSDMiddleware.Core.Models.Request;

public class IntraBankTransferRequest
{
    public string FromAccountNumber { get; set; }
    public string ToAccountNumber { get; set; }
    public string Fee { get; set; }
    public string RetrievalReference { get; set; }
    public string Narration { get; set; }
    public string Amount { get; set; }
}