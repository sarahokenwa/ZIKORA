using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface ICustomerDebitRepository
    {
        Task<CustomerDebit> LogCustomerDebit(CustomerDebit customerDebit);
        Task<CustomerDebit> UpdateCustomerDebit(CustomerDebit model, string providerId);
    }
}
