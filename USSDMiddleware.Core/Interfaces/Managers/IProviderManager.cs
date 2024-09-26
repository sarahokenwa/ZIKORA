using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Interfaces.Managers;

public interface IProviderManager
{
    Task<Provider> GetProviderByName(string provider);
}