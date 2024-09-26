using Aornis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class BillsRepository : IBillsRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<AccountRepository> _log;


        public BillsRepository(DataEntities dbContext, ILogger<AccountRepository> log)
        {
            _dbContext = dbContext;
            _log = log;
        }

        public async Task<BillsPayment> LogBillPayment(BillsPayment bill)
        {
            try
            {
                var billPayment = await _dbContext.BillsPayments.AddAsync(bill);

                await _dbContext.SaveChangesAsync();

                return billPayment.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save the bill payment: {SerializeObject}", JsonConvert.SerializeObject(bill));
                throw;
            }
        }

        public async Task<BillsPayment> UpdateBillPayment(BillsPayment model, string providerId)
        {
            try
            {
               var bill= await _dbContext.BillsPayments.FirstOrDefaultAsync(u => u.merchantref == model.merchantref && u.ProviderId == providerId);

                if(bill != null)
                {
                    bill.requeryresponsecode = model.requeryresponsecode;
                    bill.processorRef = model.processorRef;
                    
                    await _dbContext.SaveChangesAsync();
                }              

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save the bill payment: {SerializeObject}", JsonConvert.SerializeObject(model));
                throw;
            }
        }
    }
}
