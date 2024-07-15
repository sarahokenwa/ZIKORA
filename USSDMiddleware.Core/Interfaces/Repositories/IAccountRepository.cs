using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> CreateNewAccount(Account newAccount);
        Task<Account> GetCustomerById(string customerID);
        Task<Account> AddAccountToCustomer(Account newAccount);

    }
}
