using Aornis;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<Optional<User>> GetByPhoneNumber(string phoneNumber, string providerId);
        Task<User> CreateUser(User user);
    }
}
