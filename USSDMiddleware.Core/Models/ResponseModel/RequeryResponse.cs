namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class RequeryResponse
    {
            public bool IsSuccessful { get; set; }
            public string ResponseMessage { get; set; }
            public string ResponseCode { get; set; }
            public string Reference { get; set; }
            public string Status { get; set; }
        

        //public string Code { get; set; }
        //public bool Succeeded { get; set; }
        //public Data Data { get; set; }
    }

    //public class Data
    //{
    //    public string Status { get; set; }
    //    public string Message { get; set; }
    //    public string Reference { get; set; }
    //    public object TransactionDate { get; set; }
    //    public string CustomerName { get; set; }
    //    public int Amount { get; set; }
    //    public int AmountAfterCharge { get; set; }
    //    public int Charge { get; set; }
    //    public string CustomerId { get; set; }
    //    public string AccountNumber { get; set; }
    //}
}
