using USSDMiddleware.Infrastructure.Data;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Repositories;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DataEntities _dbContext; 
        private readonly ILogger<AccountRepository> _log;


        public AccountRepository(DataEntities dbContext, ILogger<AccountRepository> log)
        {
            _dbContext = dbContext;  
            _log = log;
        }

        public async Task<Account> CreateNewAccount(Account newAccount)
        {
            try
            {
                var newUserAccount = await _dbContext.Accounts.AddAsync(newAccount);

                await _dbContext.SaveChangesAsync();

                return newUserAccount.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save the new account: {SerializeObject}", JsonConvert.SerializeObject(newAccount));
                throw;
            }
        }

        public async Task<Account> GetCustomerById(string customerID)
        {
            return await _dbContext.Accounts.FirstOrDefaultAsync(c => c.CustomerID == customerID);
        }

        public async Task<Account> AddAccountToCustomer(Account newAccount)
        {
            try
            {
                var newUserAccount = await _dbContext.Accounts.AddAsync(newAccount);

                await _dbContext.SaveChangesAsync();

                return newUserAccount.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save the new account: {SerializeObject}", JsonConvert.SerializeObject(newAccount));
                throw;
            }
        }


    }
}
