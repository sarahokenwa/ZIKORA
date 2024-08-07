using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Accounts
{
    public class NameEnquiryRequest
    {
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
    }
}
