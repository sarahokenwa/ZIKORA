namespace USSDMiddleware.Core.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string ProviderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
    }
}
