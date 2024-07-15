namespace USSDMiddleware.Core.Models.Bills
{
    public class PaymentItemsResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public PaymentItem[] Data { get; set; }
    }

    public class PaymentItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ItemCode { get; set; }
        public string Commission { get; set; }
        public int CyberPayBillerId { get; set; }
        public string Amount { get; set; }
        public float Fee { get; set; }
        public string CommissionType { get; set; }
        public string CommissionSource { get; set; }
        public string CustomerIdHint { get; set; }
        public float Cap { get; set; }
        public int TotalAmount { get; set; }
        public bool Capped { get; set; }
    }

}
