using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using System.Runtime;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Infrastructure.Entities;

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
        private readonly IInstantPayOutRepository _instantPayOutRepository;
        private readonly ICustomerDebitRepository _customerDebitRepository;
        private readonly IUserManager _userManager;

        public PayOutManager(IPayOutService payOutService,
            ILogger<PayOutManager> log,
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            IConfiguration configuration,
            IInstantPayOutRepository instantPayOutRepository,
            ICustomerDebitRepository customerDebitRepository,
            IUserManager userManager)
        {
            _payOutService = payOutService;
            _log = log;
            _userRepository = userRepository;
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _configuration = configuration;
            _instantPayOutRepository = instantPayOutRepository;
            _customerDebitRepository = customerDebitRepository;
            _userManager = userManager;
        }

        public async Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request)
        {
            try
            {
                var settings = new ZIKORAModelExtension();
                settings.RetrievalReference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);
                string merchantReference = Guid.NewGuid().ToString();

                // Extract configuration values from appsettings.
                settings.GLCode = _configuration["ApiOptions:Zikora:GLCode"];
                settings.NibssCode = _configuration["ApiOptions:Zikora:NibssCode"];
                settings.FundTransferFee = decimal.Parse(_configuration["ApiOptions:Zikora:FundTransferFee"]);

                Interfaces.Providers.IUssdProvider provider = _providerSelector.GetProvider(request.Provider);
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

               //var userDetail = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);
                 
               var transactionPin = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
               if(transactionPin) 
                {
                        var debitRequest = new DebitCustomerAccountRequest
                        {
                            RetrievalReference = settings.RetrievalReference,
                            AccountNumber = request.SenderAccountNumber,
                            Amount = request.Amount.ToString(),
                            Narration = $"Debit Customer account to {request.BeneficiaryAccountName}",
                        };

                        CustomerDebit logdebitRequest = await LogCustomerDebit(request, settings, providerId);

                        DebitCustomerAccountResponse debitResponse = await provider.DebitCustomerAccount(debitRequest);


                        CustomerDebit updateCustomerDebit = await UpdateCustomerDebit(debitResponse, logdebitRequest, providerId);

                        if (debitResponse.Succeeded)
                        {

                            FundTransfer logInstantPayOut = await LogInstantPayment(request, merchantReference, providerId);


                            var instantPayOut = await _payOutService.InstantPayOut(request);
                            if (instantPayOut.Succeeded && instantPayOut.Data != null)
                            {
                                logInstantPayOut.ProcessorRef = instantPayOut.Data.Data;
                                instantPayOut.Code = "200";
                            }
                            else
                            {
                                instantPayOut.Code = "500";
                            }

                            var updateinstantPayOut = await _instantPayOutRepository.UpdateInstantPayment(logInstantPayOut, providerId);
                            return instantPayOut;

                        }
                } 
               //if(userDetail.Value.TransactionPin == request.TransactionPin)
               // {
               //     var debitRequest = new DebitCustomerAccountRequest
               //     {
               //         RetrievalReference = settings.RetrievalReference,
               //         AccountNumber = request.SenderAccountNumber,
               //         Amount = request.Amount.ToString(),
               //         Narration = $"Debit Customer account to {request.BeneficiaryAccountName}",
               //     };

               //     CustomerDebit logdebitRequest = await LogCustomerDebit(request, settings, providerId);

               //     DebitCustomerAccountResponse debitResponse = await provider.DebitCustomerAccount(debitRequest);

                    
               //     CustomerDebit updateCustomerDebit = await UpdateCustomerDebit(debitResponse, logdebitRequest, providerId);
                    
               //     if (debitResponse.Succeeded)
               //     {

               //         FundTransfer logInstantPayOut = await LogInstantPayment(request, merchantReference, providerId);


               //         var instantPayOut = await _payOutService.InstantPayOut(request);
               //         if (instantPayOut.Succeeded && instantPayOut.Data != null)
               //         {
               //             logInstantPayOut.ProcessorRef = instantPayOut.Data.Data;
               //             instantPayOut.Code = "200"; 
               //         }
               //         else
               //         {
               //             instantPayOut.Code = "500";
               //         }

               //         var updateinstantPayOut = await _instantPayOutRepository.UpdateInstantPayment(logInstantPayOut, providerId);
               //         return instantPayOut;

               //     }
               // }

                return new InstantPayOutResponse
                {
                    Data = new InstantPayOutResponse.DataResponse
                    {
                        SessionId = "null",
                        Succeeded = false,
                        Code = "Failed",
                        Message = "Instant payout failed.",
                        Data = null
                    }
                };

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Instant payout failed.");
            }
        }

        public async Task<CustomerDebit> LogCustomerDebit(InstantPayOutRequest request, ZIKORAModelExtension settings, string providerId)
        {
            return await _customerDebitRepository.LogCustomerDebit(Builder<CustomerDebit>.CreateNew()
              .With(d => d.RetrievalReference = settings.RetrievalReference)
              .With(d => d.AccountNumber = request.SenderAccountNumber)
              .With(d => d.BankCode = request.BankCode)
              .With(d => d.ProviderId = providerId)
              .With(d => d.Amount = request.Amount)
              .With(d => d.TransactionPin = request.TransactionPin)
              .With(d => d.Narration = request.Narration)
              .With(d => d.GLCode = settings.GLCode)
              .With(d => d.NibssCode = settings.NibssCode)
              .With(d => d.Fee = settings.FundTransferFee ?? 0.00m)
              .With(d => d.CreatedOn = DateTime.Now)
            .With(d => d.UpdatedOn = DateTime.Now)
            .Build());

        }

        public async Task<CustomerDebit> UpdateCustomerDebit(DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest, string providerId)
        {
            if (debitResponse.Succeeded && debitResponse.Data != null)
            {
                logdebitRequest.ProcessorRef = debitResponse.Data.Reference;
                //Can't access the ResponseDataProperty from here.
                //logdebitRequest.responsecode = "Successfull";


            }
            else
            {

                //Can't access the ResponseDataProperty from here.
                // logdebitRequest.responsecode = "Failed";
            }
            return await _customerDebitRepository.UpdateCustomerDebit(logdebitRequest, providerId);

        }

        public async Task<FundTransfer> LogInstantPayment(InstantPayOutRequest request, string merchantReference, string providerId)
        {
            return await _instantPayOutRepository.LogInstantPayment(Builder<FundTransfer>.CreateNew()
                     .With(u => u.WalletCode = _configuration["ApiOptions:WalletCode"])
                     .With(u => u.SenderAccountNumber = request.SenderAccountNumber)
                     .With(u => u.SenderAccountName = request.SenderAccountName)
                     .With(u => u.BeneficiaryAccountName = request.BeneficiaryAccountName)
                     .With(u => u.BeneficiaryAccountNumber = request.BeneficiaryAccountNumber)
                     .With(u => u.BankCode = request.BankCode)
                     .With(u => u.ProviderId = providerId)
                     .With(u => u.Amount = request.Amount)
                     .With(u => u.PhoneNumber = request.PhoneNumber)
                     .With(u => u.TransactionPin = request.TransactionPin)
                     .With(u => u.Narration = request.Narration)
                     .With(u => u.MerchantRef = merchantReference)
                     .With(u => u.MerchantCharge = decimal.Parse(_configuration["ApiOptions:MerchantCharge"]))
                     .With(u => u.WebHook = _configuration["ApiOptions:WebHook"])
                     .With(u => u.WalletType = _configuration["ApiOptions:WalletType"])
                     .With(u => u.CreatedOn = DateTime.Now)
                     .With(u => u.UpdatedOn = DateTime.Now)

                     .Build());
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

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to retrieve banks.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Get all Banks failed.");
            }
        }


    }
}
