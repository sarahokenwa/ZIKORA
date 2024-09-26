using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Accounts
{
    public class AccountBalanceEnquiry
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Balance { get; set; }
    }
}
