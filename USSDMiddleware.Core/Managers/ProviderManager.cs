using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Managers;

public class ProviderManager : IProviderManager
{
    private readonly IProviderRepository _providerRepository;


    public ProviderManager(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<Provider> GetProviderByName(string name)
    {
        var provider = await _providerRepository.GetProvider(name);
        
        if (!provider.HasValue)
        {
            throw new NotFoundException("No provider was found for this request!");
        }

        return provider.Value;
    }
}