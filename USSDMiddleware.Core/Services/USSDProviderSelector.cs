
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Interfaces.Providers;

namespace USSDMiddleware.Core.Services
{
    public class UssdProviderSelector
    {
        private readonly IEnumerable<IUssdProvider> _providers;

        public UssdProviderSelector(IEnumerable<IUssdProvider> providers)
        {
            _providers = providers;
        }

        public IUssdProvider GetProvider(Providers providerType)
        {
             return (_providers.FirstOrDefault(p => p.ProviderType == providerType) ?? _providers.FirstOrDefault(p => p.ProviderType == Providers.ZIKORA));
        }
    }
}

