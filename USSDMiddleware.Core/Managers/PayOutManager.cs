using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Bills;
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
        private readonly IInstantPayOutRepository _instantPayOutRepository;
        private readonly ICustomerDebitRepository _customerDebitRepository;
        private readonly IUserManager _userManager;
        private readonly IBackgroundService _backgroundService;
        private readonly IIntraBankTransferRepository _intraBankTransferRepository;


        public PayOutManager(IPayOutService payOutService,
            ILogger<PayOutManager> log,
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            IConfiguration configuration,
            IInstantPayOutRepository instantPayOutRepository,
            ICustomerDebitRepository customerDebitRepository,
            IUserManager userManager,
            IBackgroundService backgroundService,
            IIntraBankTransferRepository intraBankTransferRepository)
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
            _backgroundService = backgroundService;
            _intraBankTransferRepository = intraBankTransferRepository;
        }

        public async Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequestExtension request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Phonenumber is required",
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

                if (request.Amount < 1)
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Invalid transasction amount",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.SenderName))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Sender name is required",
                        Succeeded = false
                    };
                }

                if (string.IsNullOrEmpty(request.AccountNumber))
                {
                    return new InstantPayOutResponse
                    {
                        Message = "Account number is required",
                        Succeeded = false
                    };
                }

                //Converting amount to kobo
                request.Amount = request.Amount * 100;

                var response = new InstantPayOutResponse();

                var settings = new ZIKORAModelExtension();
                settings.RetrievalReference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);
                string merchantReference = settings.RetrievalReference;

                // Extract configuration values from appsettings.
                settings.GLCode = _configuration["ApiOptions:Zikora:GLCode"];
                settings.NibssCode = _configuration["ApiOptions:Zikora:NibssCode"];
                settings.FundTransferFee = decimal.Parse(_configuration["ApiOptions:Zikora:FundTransferFee"]);

                Interfaces.Providers.IUssdProvider provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                var transactionPin = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
                if (transactionPin)
                {

                    CustomerDebit customerDebit = new CustomerDebit
                    {
                        Amount = request.Amount,
                        AccountNumber = request.SenderAccountNumber,
                        RetrievalReference = settings.RetrievalReference,
                        Narration = request.Narration,
                        GLCode = settings.GLCode,
                        NibssCode = settings.NibssCode,
                        ProviderId = providerId,
                        BankCode = settings.BankCode,
                    };

                    var debitRequest = new DebitCustomerAccountRequest
                    {
                        RetrievalReference = settings.RetrievalReference,
                        AccountNumber = request.SenderAccountNumber,
                        Amount = request.Amount.ToString(),
                        Narration = $"Debit Customer account to {request.BeneficiaryName}",
                    };

                    CustomerDebit logdebitRequest = await LogCustomerDebit(customerDebit);

                    DebitCustomerAccountResponse debitResponse = await provider.DebitCustomerAccount(debitRequest);
                    CustomerDebit updateCustomerDebit = await UpdateCustomerDebit(debitResponse, logdebitRequest, providerId);

                    if (debitResponse != null && debitResponse.IsSuccessful)
                    {
                        await _backgroundService.EnqueueProcess(() => InstantPayOutBill(debitResponse, logdebitRequest, request, merchantReference));
                        response.Succeeded = true;
                        response.Message = "Request is being processed.";

                    }
                    else
                    {
                        response.Succeeded = false;
                        response.Message = debitResponse.ResponseMessage;

                    }
                }

                return response;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Instant payout failed.");
            }
        }

        public async Task<InstantPayOutResponse> InstantPayOutBill(DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest,
                                                                   InstantPayOutRequestExtension request, string merchantReference)
        {
            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);

            if (!debitResponse.ResponseCode.Equals("00"))
            {
                return new InstantPayOutResponse
                {
                    Succeeded = false,
                    Message = "Failed to debit customer account, insufficient fund."
                };
            }
            ReQueryRequest requeryPayload = new ReQueryRequest
            {
                RetrievalReference = merchantReference,
                Amount = request.Amount
            };

            var requery = await provider.StatusQuery(requeryPayload);
            if (requery != null && !requery.ResponseCode.Equals("00"))
            {
                return new InstantPayOutResponse
                {
                    Succeeded = false,
                    Message = "Requery failed."
                };
            }

            FundTransfer logInstantPayOut = await LogInstantPayment(request, merchantReference, providerId);


            var instantPayOut = await _payOutService.InstantPayOut(request, merchantReference);
            if (instantPayOut.Succeeded && instantPayOut.Data != null)
            {
                logInstantPayOut.ProcessorRef = instantPayOut.Data;
                logInstantPayOut.Data = instantPayOut.Data;
                logInstantPayOut.Succeeded = true;
                logInstantPayOut.Code = instantPayOut.Code;
                logInstantPayOut.SessionId = instantPayOut.SessionId;
                logInstantPayOut.Message = instantPayOut.Message;

            }
            else
            {
                logInstantPayOut.Code = instantPayOut.Code;
            }

            var updateinstantPayOut = await _instantPayOutRepository.UpdateInstantPayment(logInstantPayOut, providerId);
            return instantPayOut;

        }

        public async Task<CustomerDebit> LogCustomerDebit(CustomerDebit request)
        {
            return await _customerDebitRepository.LogCustomerDebit(Builder<CustomerDebit>.CreateNew()
              .With(d => d.RetrievalReference = request.RetrievalReference)
              .With(d => d.AccountNumber = request.AccountNumber)
              .With(d => d.BankCode = request.BankCode)
              .With(d => d.ProviderId = request.ProviderId)
              .With(d => d.Amount = request.Amount)
              .With(d => d.Narration = request.Narration)
              .With(d => d.GLCode = request.GLCode)
              .With(d => d.NibssCode = request.NibssCode)
              .With(d => d.Fee = request.Fee > 0 ? request.Fee : 0.0m)
              .With(d => d.CreatedOn = DateTime.Now)
            .With(d => d.UpdatedOn = DateTime.Now)
            .Build());

        }

        public async Task<CustomerDebit> UpdateCustomerDebit(DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest, string providerId)
        {
            if (debitResponse.IsSuccessful)
            {
                logdebitRequest.ProcessorRef = debitResponse.Reference;

            }
            else
            {
                throw new NotSuccessfulException("Failed to update customer's debit record.");

            }
            return await _customerDebitRepository.UpdateCustomerDebit(logdebitRequest, providerId);

        }

        public async Task<FundTransfer> LogInstantPayment(InstantPayOutRequestExtension request, string merchantReference, string providerId)
        {
            return await _instantPayOutRepository.LogInstantPayment(Builder<FundTransfer>.CreateNew()
                     .With(u => u.WalletCode = _configuration["ApiOptions:Zikora:WalletCode"])
                     .With(u => u.AccountNumber = request.AccountNumber)
                     .With(u => u.BeneficiaryName = request.BeneficiaryName)
                     .With(u => u.SenderName = request.SenderName)
                     .With(u => u.BankCode = request.BankCode)
                     // .With(u => u.BankCode = _configuration["ApiOptions:Zikora:BankCode"])
                     .With(u => u.ProviderId = providerId)
                     .With(u => u.Amount = request.Amount)
                     .With(u => u.PhoneNumber = request.PhoneNumber)
                     .With(u => u.TransactionPin = request.TransactionPin)
                     .With(u => u.Narration = request.Narration)
                     .With(u => u.MerchantRef = merchantReference)
                     .With(u => u.MerchantCharge = decimal.Parse(_configuration["ApiOptions:Zikora:MerchantCharge"]))
                     .With(u => u.WebHook = _configuration["ApiOptions:Zikora:WebHook"])
                     .With(u => u.WalletType = _configuration["ApiOptions:Zikora:WalletType"])
                     .With(u => u.CreatedOn = DateTime.Now)
                     .With(u => u.UpdatedOn = DateTime.Now)

                     .Build());
        }

        public async Task<IntraBankTransferResponse> IntraBankTransfer(IntraBankTransferRequestExtension request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Narration))
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = "Narration is required",
                        IsSuccessful = false
                    };
                }

                if (string.IsNullOrEmpty(request.FromAccountNumber))
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = "Sender account number is required",
                        IsSuccessful = false
                    };
                }

                if (string.IsNullOrEmpty(request.ToAccountNumber))
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = "Beneficiary account number is required",
                        IsSuccessful = false
                    };
                }

                if (request.Amount < 1)
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = "Invalid transasction amount",
                        IsSuccessful = false
                    };
                }

                //Converting amount to kobo
                request.Amount = request.Amount * 100;
                var settings = new ZIKORAModelExtension();
                settings.RetrievalReference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);

                Interfaces.Providers.IUssdProvider provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                var fromUser = await provider.GetUserByAccountNumber(request.FromAccountNumber);
                if (fromUser == null)
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = $"Sender with account number {request.FromAccountNumber} does not exist.",
                        IsSuccessful = false
                    };
                }

                var toUser = await provider.GetUserByAccountNumber(request.ToAccountNumber);
                if (toUser == null)
                {
                    return new IntraBankTransferResponse
                    {
                        ResponseMessage = $"Beneficiary with account number {request.ToAccountNumber} does not exist.",
                        IsSuccessful = false
                    };
                }
                var transactionPin = await _userManager.ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
                if (transactionPin == null)
                {
                    return new IntraBankTransferResponse { ResponseMessage = "Invalid phone number or pin.", IsSuccessful = false };

                }

                var intraBankTransfer = new IntraBankTransfer
                {
                    FromAccountNumber = request.FromAccountNumber,
                    ToAccountNumber = request.ToAccountNumber,
                    Fee = decimal.Parse(_configuration["ApiOptions:Zikora:FundTransferFee"]),
                    ProviderId = providerId,
                    RetrievalReference = settings.RetrievalReference,
                    Narration = request.Narration,
                    Amount = request.Amount
                };

                var intraBankTransferRequest = new IntraBankTransferRequest
                {
                    RetrievalReference = settings.RetrievalReference,
                    FromAccountNumber = request.FromAccountNumber,
                    ToAccountNumber = request.ToAccountNumber,
                    Amount = request.Amount,
                    Fee = intraBankTransfer.Fee,
                    Narration = $"Debit Customer account to {request.ToAccountNumber}",
                };

                IntraBankTransfer logIntraBankTransfer = await LogIntraBankTransfer(intraBankTransfer);

                IntraBankTransferResponse intraBankTransferResponse = await provider.IntraBankTransfer(intraBankTransferRequest);
                IntraBankTransfer updateIntraBankTransfer = await UpdateIntraBankTransfer(intraBankTransferResponse, logIntraBankTransfer, providerId);

                return intraBankTransferResponse;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Instant payout failed.");
            }
        }

        public async Task<IntraBankTransfer> LogIntraBankTransfer(IntraBankTransfer request)
        {
            return await _intraBankTransferRepository.LogIntraBankTransfer(Builder<IntraBankTransfer>.CreateNew()
              .With(d => d.RetrievalReference = request.RetrievalReference)
              .With(d => d.ToAccountNumber = request.ToAccountNumber)
              .With(d => d.FromAccountNumber = request.FromAccountNumber)
              .With(d => d.ProviderId = request.ProviderId)
              .With(d => d.Amount = request.Amount)
              .With(d => d.Narration = request.Narration)
              .With(d => d.Fee = request.Fee > 0 ? request.Fee : 0.0m)
              .With(d => d.CreatedOn = DateTime.Now)
            .With(d => d.UpdatedOn = DateTime.Now)
            .Build());

        }

        public async Task<IntraBankTransfer> UpdateIntraBankTransfer(IntraBankTransferResponse intraBankTransferResponse, IntraBankTransfer logIntraBankTransferRequest, string providerId)
        {

            logIntraBankTransferRequest.ProcessorRef = intraBankTransferResponse.Reference;
            logIntraBankTransferRequest.ResponseCode = intraBankTransferResponse.ResponseCode;
            logIntraBankTransferRequest.ResponseMessage = intraBankTransferResponse.ResponseMessage;
            logIntraBankTransferRequest.IsSuccessful = intraBankTransferResponse.IsSuccessful;


            return await _intraBankTransferRepository.UpdateIntraBankTransfer(logIntraBankTransferRequest, providerId);

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
