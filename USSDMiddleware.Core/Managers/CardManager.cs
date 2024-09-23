using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Bills;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using Card = USSDMiddleware.Core.Entities.Card;

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
                
                var userExists = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

                if (userExists == null)
                {
                    return new CardResponse { ResponseMessage = "Invalid account number.", IsSuccessful = false };
                }
                
                var userPin = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
                if(!userPin)
                {
                    return new CardResponse { ResponseMessage = "The pin entered is incorrect.", IsSuccessful = false };

                }

                GetUserByAccountNumberResponse user = await provider.GetUserByAccountNumber(request.AccountNumber);
                if (string.IsNullOrEmpty(user.Name) || !string.IsNullOrEmpty(user.ErrorMessage))
                {
                    return new CardResponse
                    {
                        ResponseMessage = user.ErrorMessage,
                        IsSuccessful = false
                    };
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
                if (!cardResponse.IsSuccessful)
                {
                    return new CardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = cardResponse.ResponseMessage
                    };
                }

                Card updateCardRequest = await UpdateCardRequest(cardResponse, logCardRequest, providerId);

                
                return cardResponse;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"An error occurred while trying to make a card request: {ex.Message}");

                return new CardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "An error occurred while trying to make a card request"
                };
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

        public async Task<FreezeCardResponse> FreezeCard(FreezeCardRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccountNumber))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "AccountNumber  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.Reason))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Reason  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "PhoneNumber  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.TransactionPin))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Transaction pin  is required",
                    };
                }


                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                bool isPinValid = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
                if (!isPinValid)
                {
                    return new FreezeCardResponse { ResponseMessage = "The pin entered is incorrect.", IsSuccessful = false };

                }

                string reference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);
               
                
                var getCustomerCardRequest = new GetCustomerCardRequest
                {
                    AccountNo = request.AccountNumber,
                    IncludeInactiveCards = true 
                };

                GetCustomerCardResponse cardResponse = await provider.GetCustomerCards(getCustomerCardRequest);

                if (cardResponse.IsSuccessful && cardResponse.Cards?.Any() == true)
                {
                    string serialNo = cardResponse.Cards.First().SerialNo;

                    var freezeCardRequest = new FreezeCardRequest
                    {
                        SerialNo = serialNo,
                        Reference = reference,
                        Reason = request.Reason,
                        AccountNumber = request.AccountNumber,
                        Provider = request.Provider 
                    };

                    var freezeCardResponse = await provider.FreezeCard(freezeCardRequest);

                    return freezeCardResponse;

                }
                else
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "No cards found for the provided account.",
                    };
                }

            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"An error occurred while trying to freeze card: {ex.Message}");

                return new FreezeCardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = $"An error occurred while trying to freeze card."
                };
            }
        }

        public async Task<UnFreezeCardResponse> UnFreezeCard(UnFreezeCardRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccountNumber))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "AccountNumber  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.Reason))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Reason  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "PhoneNumber  is required",
                    };
                }

                if (string.IsNullOrEmpty(request.TransactionPin))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Transaction pin  is required",
                        Reference = ""
                    };
                }


                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                bool isPinValid = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
                if (!isPinValid)
                {
                    return new UnFreezeCardResponse { ResponseMessage = "The pin entered is incorrect.", IsSuccessful = false };

                }

                var reference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);

                var getCustomerCardRequest = new GetCustomerCardRequest
                {
                    AccountNo = request.AccountNumber,
                    IncludeInactiveCards = true
                };

                GetCustomerCardResponse cardResponse = await provider.GetCustomerCards(getCustomerCardRequest);

                if (cardResponse.IsSuccessful && cardResponse.Cards?.Any() == true)
                {
                    string serialNo = cardResponse.Cards.First().SerialNo;

                    request.SerialNo = serialNo;

                    var unfreezeCardResponse = await provider.UnFreezeCard(request);

                    return unfreezeCardResponse;
                }
                else
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "No card found for the provided account.",
                        Reference = reference
                    };
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"An error occurred while trying to unfreeze card: {ex.Message}");

                return new UnFreezeCardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = $"An error occurred while trying to unfreeze card."
                };

            }
        }
    }
}
