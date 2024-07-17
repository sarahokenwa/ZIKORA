using Aornis;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly DataEntities _dbContext;

    public ProviderRepository(DataEntities dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Optional<Provider>> GetProvider(string name)
    {
        return Task.FromResult(Optional.Of(_dbContext.Providers.FirstOrDefault(p => p.Name == name)));
    }
}