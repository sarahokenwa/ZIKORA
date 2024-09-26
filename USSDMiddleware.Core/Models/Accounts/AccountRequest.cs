using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Accounts
{
    public class AccountRequest
    {
        public string PhoneNumber { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionPin { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
