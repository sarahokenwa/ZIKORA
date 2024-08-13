using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;

namespace USSDMiddleware.Core.Managers
{
    public class PayOutManager : IPayOutManager
    {
        private readonly IPayOutService _payOutService;
        private readonly ILogger<PayOutManager> _log;
        private readonly IUserRepository _userRepository;
        private readonly UssdProviderSelector _providerSelector;
        private readonly IProviderManager _providerManager;
        private readonly IConfiguration _configuration;

        public PayOutManager(IPayOutService payOutService,
            ILogger<PayOutManager> log,
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            IConfiguration configuration)
        {
            _payOutService = payOutService;
            _log = log;
            _userRepository = userRepository;
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _configuration = configuration;
        }

        public async Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request)
        {
            try
            {
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Phonenumber is required",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.BeneficiaryAccountNumber))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Beneficiary account number is required",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.BeneficiaryAccountName))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Beneficiary account name is required",
                        Succeeded = false
                    };
                }

                if (request.Amount < 1)
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Invalid transasction amount",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.SenderAccountName))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Sender account name is required",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.SenderAccountNumber))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Sender account number is required",
                        Succeeded = false
                    };
                }

               var userDetail = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

               if(userDetail.Value.TransactionPin == request.TransactionPin)
                {
                    var debitRequest = new DebitCustomerAccountRequest
                    {
                        RetrievalReference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12),
                        AccountNumber = request.SenderAccountNumber,
                        NibssCode = _configuration["ApiOptions:NibssCode"], 
                        Amount = request.Amount.ToString(),
                        Fee = _configuration["ApiOptions:FundTransferFee"], 
                        Narration = $"Debit Customer account to {request.BeneficiaryAccountName}",
                        GLCode = _configuration["ApiOptions:GLCode"]
                    };
                    var debitResponse = await provider.DebitCustomerAccount(debitRequest);
                    if(debitResponse.IsSuccessful)
                    {
                        var instantPayOut = await _payOutService.InstantPayOut(request);
                        return instantPayOut;

                    }
                    

                   
                }
                return new InstantPayOutResponse
                {
                    sessionId = "6789qwerhbgfdctruikjnhft", //Write a code to generate this.
                    Succeeded = false,
                    Code = "INVALID_PIN",
                    Message = "Invalid transaction PIN",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Instant payout failed.");
            }
        }

        public async Task<RequeryResponse> RequeryPayOut(string reference)
        {
            try
            {
                var requeryResponse = await _payOutService.RequeryPayOut(reference);

                return requeryResponse;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to requery instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Requery failed.");
            }
        }

        public async Task<BankResponseDto[]> Get()
        {
            try
            {
                var bankResponse = await _payOutService.Get();

                if (bankResponse == null || bankResponse.Data == null || !bankResponse.Data.Any())
                {
                    _log.LogError("Failed to retrieve bank data.");
                    throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Failed to retrieve bank data.");
                }

                var bankList = bankResponse.Data.Select(b => new BankResponseDto
                {
                    Id = b.Id,
                    BankCode = b.BankCode,
                    BankName = b.BankName
                }).ToArray();
                return bankList;

                //return new BankResponse
                //{
                //    Code = bankResponse.Code,
                //    Succeeded = bankResponse.Succeeded,
                //    Data = bankList
                //};
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to retrieve banks.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Get all Banks failed.");
            }
        }


    }
}
