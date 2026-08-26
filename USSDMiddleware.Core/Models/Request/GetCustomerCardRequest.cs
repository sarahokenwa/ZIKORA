namespace USSDMiddleware.Core.Models.Request
{
    public class GetCustomerCardRequest
    {
        public string AccountNo { get; set; }
        public string? CustomerId { get; set; }   
        public bool IncludeInactiveCards { get; set; }
    }
}
