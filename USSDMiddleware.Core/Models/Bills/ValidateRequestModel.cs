using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Infrastructure.Entities;
using USSDMiddleware.Core.Enums;
namespace USSDMiddleware.Core.Models.Bills
{
  

    public class ValidateRequestModel
    {
        public string itemCode { get; set; }
        public string customerId { get; set; }
        public string customerPhoneNumber { get; set; }
        public bool shouldVerifyCustomer { get; set; }
        public string customerEmail { get; set; }
        public string customerName { get; set; }
        public decimal amount { get; set; }
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }
}
