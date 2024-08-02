using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IBillsRepository
    {
        Task<BillsPayment> LogBillPayment(BillsPayment bill);
      //  Task<BillsPayment> GetBillPaymentByPhoneNumber(string phoneNumber);
        Task<BillsPayment> UpdateBillPayment(BillsPayment model, string providerId);
    }
}
