using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Bills
{
    public class ClientVendRequest
    {
        public string PaymentCode { get; set; }
        public decimal Amount { get; set; }
        public string CustomerId { get; set; }
        public string CustomerMobile { get; set; }
    }
}
