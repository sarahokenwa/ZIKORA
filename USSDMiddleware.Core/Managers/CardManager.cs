using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;

namespace USSDMiddleware.Core.Managers
{
    public class CardManager : ICardManager
    {
        private readonly UssdProviderSelector _providerSelector;
        private readonly IProviderManager _providerManager;
        private readonly ILogger<CardManager> _log; 
        private readonly IConfiguration _configuration;


        public CardManager(UssdProviderSelector providerSelector,
                           ILogger<CardManager> log,
                           IProviderManager providerManager,
                           IConfiguration configuration) 
        {
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _log = log;
            _configuration = configuration;
        }

        public async Task<CardResponse> CardRequest(CardRequest request)
        {
            try
            {
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                var cardRequestExtension = new CardRequestExtension
                {
                    AccountNumber = request.AccountNumber,
                    Provider = request.Provider,
                    BIN = _configuration["ApiOptions:Zikora:BIN"],
                    RequestType = _configuration["ApiOptions:Zikora:RequestType"],
                    DeliveryOption = _configuration["ApiOptions:Zikora:DeliveryOption"],
                    Identifier = _configuration["ApiOptions:Zikora:Identifier"],
                    NameOnCard = (await provider.GetUserByAccountNumber(request.AccountNumber)).Name
                };

                var cardResponse = await provider.CardRequest(cardRequestExtension);

                return cardResponse;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to initiate a card request.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Card Initiation failed.");
            }
        }
    }
}
