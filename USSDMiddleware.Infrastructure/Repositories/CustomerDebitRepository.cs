using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class CustomerDebitRepository : ICustomerDebitRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<CustomerDebitRepository> _log;

        public CustomerDebitRepository(DataEntities dbContext, ILogger<CustomerDebitRepository> log) 
        { 
            _dbContext = dbContext;
            _log = log;
        }

        public async Task<CustomerDebit> LogCustomerDebit(CustomerDebit customerDebit)
        {
            try
            {
                var debitCustomer = await _dbContext.CustomerDebits.AddAsync(customerDebit); 

                await _dbContext.SaveChangesAsync();

                return debitCustomer.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save customer debit: {SerializeObject}", JsonConvert.SerializeObject(customerDebit));
                throw;
            }
        }

        public async Task<CustomerDebit> UpdateCustomerDebit(CustomerDebit model, string providerId)
        {
            try
            {
                var debitCustomer = await _dbContext.CustomerDebits.FirstOrDefaultAsync(u => u.RetrievalReference == model.RetrievalReference && u.ProviderId == providerId);

                if (debitCustomer != null)
                {
                   // debitCustomer.
                    // instantPayment.requeryresponsecode = model.requeryresponsecode;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to update customer debit: {SerializeObject}", JsonConvert.SerializeObject(model));
                throw;
            }
        }

    }
}
