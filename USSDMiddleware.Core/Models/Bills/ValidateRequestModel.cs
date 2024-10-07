namespace USSDMiddleware.Core.Models.Bills
{

    public class ValidateRequestModel
    {
        public string itemCode { get; set; }
        public string customerId { get; set; }
        public string customerPhoneNumber { get; set; }
        public bool phoneValidation { get; set; }
        public string customerEmail { get; set; }
        public string customerName { get; set; }
        public decimal amount { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
