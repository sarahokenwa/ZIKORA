using Aornis;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories;

public interface IProviderRepository
{
    Task<Optional<Provider>> GetProvider(string name);
}