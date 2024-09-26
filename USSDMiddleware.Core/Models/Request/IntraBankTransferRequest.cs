namespace USSDMiddleware.Core.Models.Request;

public class IntraBankTransferRequest
{
    public string FromAccountNumber { get; set; }
    public string ToAccountNumber { get; set; }
    public decimal? Fee { get; set; }
    public string? RetrievalReference { get; set; }
    public string Narration { get; set; }
    public decimal Amount { get; set; }
    public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

}