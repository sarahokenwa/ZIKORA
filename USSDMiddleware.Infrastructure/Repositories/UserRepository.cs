using Aornis;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataEntities _dbContext;
        

        public UserRepository(DataEntities dbContext)
        {
            _dbContext = dbContext;
           
        }


        public Task<Optional<User>> GetByPhoneNumber(string phoneNumber)
        {
              return Task.FromResult(Optional.Of(_dbContext.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber)));
        }

    }
}
