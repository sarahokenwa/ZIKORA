using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
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
                _log.LogError($"Failed to save customer debit: {JsonConvert.SerializeObject(customerDebit)}");
                throw new NotSuccessfulException($"Failed to save customer debit: {ex.Message}");
            }
        }

        public async Task<CustomerDebit> UpdateCustomerDebit(CustomerDebit model, string providerId)
        {
            try
            {
                var debitCustomer = await _dbContext.CustomerDebits.FirstOrDefaultAsync(u => u.RetrievalReference == model.RetrievalReference && u.ProviderId == providerId);

                if (debitCustomer != null)
                {
                    debitCustomer.ProcessorRef = model.RetrievalReference;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to update customer debit:  {JsonConvert.SerializeObject(model)}");
                throw new NotSuccessfulException($"Failed to update customer debit: {ex.Message}");
                ;
            }
        }

    }
}
