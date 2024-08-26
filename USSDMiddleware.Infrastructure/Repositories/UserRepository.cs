using Aornis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<AccountRepository> _log;
        

        public UserRepository(DataEntities dbContext, ILogger<AccountRepository> log)
        {
            _dbContext = dbContext;
            _log = log;
        }


        public Task<Optional<User>> GetByPhoneNumber(string phoneNumber, string providerId)
        {
              return Task.FromResult(Optional.Of(_dbContext.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.ProviderId == providerId)));
        }
        
        public async Task<User> CreateUser(User user)
        {
            try
            {
                var newUser = await _dbContext.Users.AddAsync(user);

                await _dbContext.SaveChangesAsync();

                return newUser.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save the new user with phoneNumber: {SerializeObject}", user.PhoneNumber);
                 throw;
            }
        }

        public async Task<User> GetUserByAccountNumber(string accountNumber)
        {
            var user = await _dbContext.Set<User>().Where(a=>a.AccountNumber == accountNumber).FirstOrDefaultAsync();
            if(user == null)
            {
                throw new NotFoundException("User not found.");
            }
            return user;
        }

    }
}
