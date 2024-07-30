using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Bills
{
  
    public class ValidateRequest
    {
        public string itemCode { get; set; }
        public string customerId { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string customerName { get; set; }
        public bool phoneValidation { get; set; }
       
    }

    public class ValidateResponse
    {
        public bool succeeded { get; set; }
        public string Message { get; set; }
        public ValidateResponseData Data { get; set; }
    }
    public class ValidateResponseData
    {
        public string Name { get; set; }
        public string ProviderRef { get; set; }
        public string ValidationRef { get; set; }
        public string AccountNumber { get; set; }
        public string Address { get; set; }
    }
}
