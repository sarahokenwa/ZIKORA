namespace USSDMiddleware.Core.Models.Request
{
    public class ReQueryRequest
    {
        public string RetrievalReference { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}
