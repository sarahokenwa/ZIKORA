using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.PayOut;
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
        private readonly IUserManager _userManager;
        private readonly ICardRepository _cardRepository;


        public CardManager(UssdProviderSelector providerSelector,
                           ILogger<CardManager> log,
                           IProviderManager providerManager,
                           IConfiguration configuration,
                           IUserRepository userRepository,
                           IUserManager userManager,
                           ICardRepository cardRepository) 
        {
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _log = log;
            _configuration = configuration;
            _userRepository = userRepository;
            _userManager = userManager;
            _cardRepository = cardRepository;
        }

        public async Task<CardResponse> CardRequest(CardRequest request)
        {
            try
            {
                var settings = new CardRequestExtension();

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
                //TODO: Create a table called Account
                //var user = await _userManager.ValidateTransactionPin(request.TransactionPin);
                var userExists = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

                if (userExists == null)
                {
                    throw new NotFoundException("Invalid account number.");
                }
                
                var userPin = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);

                var user = await provider.GetUserByAccountNumber(request.AccountNumber);
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }
                var cardRequestExtension = new CardRequestExtension
                {
                    AccountNumber = request.AccountNumber,
                    PhoneNumber = request.PhoneNumber,
                    Provider = request.Provider,
                    BIN = _configuration["ApiOptions:Zikora:BIN"],
                    RequestType = _configuration["ApiOptions:Zikora:RequestType"],
                    DeliveryOption = _configuration["ApiOptions:Zikora:DeliveryOption"],
                    Identifier = _configuration["ApiOptions:Zikora:Identifier"],
                    NameOnCard = user.Name,
                };
                request.NameOnCard = user.Name;
                Card logCardRequest = await LogCardRequest(request, settings, providerId);


                CardResponse cardResponse = await provider.CardRequest(cardRequestExtension);
                if (cardResponse == null || !cardResponse.IsSuccessful)
                {
                    throw new NotSuccessfulException("Card response is empty or missing.");
                }

                Card updateCardRequest = await UpdateCardRequest(cardResponse, logCardRequest, providerId);

                
                return cardResponse;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make a card request.");
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<Card> LogCardRequest(CardRequest request, CardRequestExtension settings, string providerId)
        {
            return await _cardRepository.LogCardRequest(Builder<Card>.CreateNew()
              .With(d => d.AccountNumber = request.AccountNumber)
              .With(d => d.PhoneNumber = request.PhoneNumber)
              .With(d => d.TransactionPin = request.TransactionPin)
              .With(d => d.ProviderId = providerId)
              .With(d => d.NameOnCard = request.NameOnCard)
              .With(u => u.BIN = _configuration["ApiOptions:Zikora:BIN"])
              .With(u => u.RequestType = _configuration["ApiOptions:Zikora:RequestType"])
              .With(u => u.DeliveryOption = _configuration["ApiOptions:Zikora:DeliveryOption"])
              .With(u => u.Identifier = _configuration["ApiOptions:Zikora:Identifier"])
              .With(d => d.CreatedOn = DateTime.Now)
              .With(d => d.UpdatedOn = DateTime.Now)
            .Build());
         }

        public async Task<Card> UpdateCardRequest(CardResponse cardResponse, Card logCardRequest, string providerId)
        {
            if (cardResponse.IsSuccessful && cardResponse != null)
            {
                logCardRequest.ProcessorRef = cardResponse.BatchNo;
                logCardRequest.Identifier = cardResponse.Identifier;
                logCardRequest.IsSuccessful = cardResponse.IsSuccessful;
                logCardRequest.ResponseMessage = cardResponse.ResponseMessage;
                logCardRequest.BatchNo = cardResponse.BatchNo;
                
            }
            else
            {
                logCardRequest.IsSuccessful = cardResponse.IsSuccessful;
            }
            return await _cardRepository.UpdateCardRequest(logCardRequest, providerId);

        }

        public async Task<Card> LogCardResponse(CardResponse cardResponse)
        {
            return await _cardRepository.LogCardResponse(Builder<Card>.CreateNew()
              .With(c => c.IsSuccessful = cardResponse.IsSuccessful)
              .With(c => c.ResponseMessage = cardResponse.ResponseMessage)
              .With(c => c.BatchNo = cardResponse.BatchNo)
              .With(c => c.Identifier = cardResponse.Identifier)
              .Build());
        }
    }
}
