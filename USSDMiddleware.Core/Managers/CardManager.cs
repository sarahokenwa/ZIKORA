using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
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
        private readonly IUserRepository _userRepository;


        public CardManager(UssdProviderSelector providerSelector,
                           ILogger<CardManager> log,
                           IProviderManager providerManager,
                           IConfiguration configuration,
                           IUserRepository userRepository) 
        {
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _log = log;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<CardResponse> CardRequest(CardRequest request)
        {
            try
            {
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return new CardResponse
                    {
                        ResponseMessage = "Phone number is required",
                        IsSuccessful = false
                    };
                }

                if (string.IsNullOrEmpty(request.AccountNumber))
                {
                    return new CardResponse
                    {
                        ResponseMessage = "Account number is required",
                        IsSuccessful = false
                    };
                }

                var userDetail = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);
                if (userDetail.Value.TransactionPin != request.TransactionPin)
                {
                    return new CardResponse
                    {
                        ResponseMessage = "Invalid Transaction Pin",
                        IsSuccessful = false
                    };
                }

                var cardRequestExtension = new CardRequestExtension
                {
                    AccountNumber = request.AccountNumber,
                    PhoneNumber = request.PhoneNumber,
                    Provider = request.Provider,
                    TransactionPin = request.TransactionPin,
                    BIN = _configuration["ApiOptions:Zikora:BIN"],
                    RequestType = _configuration["ApiOptions:Zikora:RequestType"],
                    DeliveryOption = _configuration["ApiOptions:Zikora:DeliveryOption"],
                    Identifier = _configuration["ApiOptions:Zikora:Identifier"],
                    NameOnCard = (await provider.GetUserByAccountNumber(request.AccountNumber)).Name
                };

                var cardResponse = await provider.CardRequest(cardRequestExtension);
                if (!cardResponse.IsSuccessful)
                {
                    _log.LogError("Failed to make card request: {ResponseMessage}", cardResponse.ResponseMessage);
                    return new CardResponse
                    {
                        ResponseMessage = "Failed to make card request: " + cardResponse.ResponseMessage,
                        IsSuccessful = false
                    };
                }

                return cardResponse;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make a card request.");
                return new CardResponse
                {
                    ResponseMessage = "An unexpected error occurred.",
                    IsSuccessful = false
                };
            }
        }
    }
}
