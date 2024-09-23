using Aornis;
using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Bills;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;

namespace USSDMiddleware.Core.Managers
{
    public class BillsManager : IBillsManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ICyberPayProvider _cyberPayProvider;
        private readonly UssdProviderSelector _providerSelector;
        private readonly ILogger<BillsManager> _log;
        private readonly IProviderManager _providerManager;
        private readonly IBillsRepository _billsRepository;
        private readonly IUserManager _userManager;
        private readonly IPayOutManager _payOutManager;
        private readonly IConfiguration _configuration;
        private readonly IBackgroundService _backgroundService;


        public BillsManager(IUserRepository userRepository,
            ICyberPayProvider cyberPayProvider,
            ILogger<BillsManager> log,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            IBillsRepository billsRepository,
            IUserManager userManager,
            IPayOutManager payOutManager,
            IConfiguration configuration,
            IBackgroundService backgroundService)
        {
            _userRepository = userRepository;
            _cyberPayProvider = cyberPayProvider;
            _providerSelector = providerSelector;
            _log = log;
            _providerManager = providerManager;
            _billsRepository = billsRepository;
            _userManager = userManager;
            _payOutManager = payOutManager;
            _configuration = configuration;
            _backgroundService = backgroundService;
        }

        public async Task<BillersResponse> GetBillers(string categoryId)
        {
            return await _cyberPayProvider.GetBillers(categoryId);
        }

        public async Task<CategoriesResponse> GetCategories(string categoryType)
        {
            return await _cyberPayProvider.GetCategories(categoryType);
        }

        public async Task<PaymentItemsResponse> GetPaymentItems(string billerId)
        {
            return await _cyberPayProvider.GetPaymentItems(billerId);
        }

        public async Task<ValidateResponse> Validate(ValidateRequestModel requestModel)
        {
            var provider = _providerSelector.GetProvider(requestModel.Provider);
            var providerId = await provider.GetProviderId(_providerManager);


            if (string.IsNullOrEmpty(requestModel.customerPhoneNumber))
            {
                return new ValidateResponse
                {
                    Message = "Customer mobile is required",
                    succeeded = false
                };
            }

            if (string.IsNullOrEmpty(requestModel.itemCode))
            {
                return new ValidateResponse
                {
                    Message = "Item code is required",
                    succeeded = false
                };
            }

            if (string.IsNullOrEmpty(requestModel.customerId))
            {
                return new ValidateResponse
                {
                    Message = "Customer ID is required",
                    succeeded = false
                };
            }

            var userDetail = await _userRepository.GetByPhoneNumber(requestModel.customerPhoneNumber, providerId);

            if (userDetail == null)
            {
                return new ValidateResponse
                {
                    Message = "Request from an invalid customer mobile",
                    succeeded = false
                };
            }

            ValidateRequest request = new ValidateRequest
            {
                customerId = requestModel.customerId,
                customerName = requestModel.customerName,
                email = requestModel.customerEmail,
                itemCode = requestModel.itemCode,
                phone = requestModel.customerPhoneNumber,
                phoneValidation = requestModel.shouldVerifyCustomer,
            };
            return await _cyberPayProvider.Validate(request);
        }

        public async Task<VendResponse> Vend(ClientVendRequest requestModel)
        {
            try
            {
                var response = new VendResponse();
                var validationResult = ValidateRequest(requestModel);
                if (!validationResult.Succeeded)
                {
                    return validationResult;
                }

                var provider = _providerSelector.GetProvider(requestModel.Provider);
                var providerId = await provider.GetProviderId(_providerManager);
                bool isPinValid = await _userManager.ValidateTransactionPin(requestModel.TransactionPin, requestModel.CustomerMobile, providerId);
                if (!isPinValid)
                {
                    return new VendResponse { Message = "The pin entered is incorrect.", Succeeded = false };
                }

                var userDetail = await _userRepository.GetByPhoneNumber(requestModel.CustomerMobile, providerId);
                var userValidationResult = await ValidateUserDetail(userDetail, providerId);
                if (!userValidationResult.Succeeded)
                {
                    return userValidationResult;
                }

                var settings = new ZIKORAModelExtension();
                settings.RetrievalReference = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 12);
                settings.GLCode = _configuration["ApiOptions:Zikora:GLCode"];
                settings.NibssCode = _configuration["ApiOptions:Zikora:NibssCode"];
                settings.FundTransferFee = decimal.Parse(_configuration["ApiOptions:Zikora:FundTransferFee"]);
                settings.BankCode = _configuration["ApiOptions:Zikora:BankCode"];
                string merchantRef = settings.RetrievalReference;

                CustomerDebit customerDebit = new CustomerDebit
                {
                    Amount = requestModel.Amount,
                    AccountNumber = requestModel.DrAccountNumber,
                    RetrievalReference = settings.RetrievalReference,
                    Narration = $"Bills payment {requestModel.PaymentCode} for {requestModel.CustomerId}",
                    GLCode = settings.GLCode,
                    NibssCode = settings.NibssCode,
                    ProviderId = providerId,
                    BankCode = settings.BankCode,
                };

                CustomerDebit logdebitRequest = await _payOutManager.LogCustomerDebit(customerDebit);

                var debitRequest = new DebitCustomerAccountRequest
                {
                    RetrievalReference = settings.RetrievalReference,
                    AccountNumber = requestModel.DrAccountNumber,
                    Amount = requestModel.Amount.ToString(),
                    Narration = $"Debit Customer account {requestModel.DrAccountNumber}  for {requestModel.CustomerId}",
                };

                DebitCustomerAccountResponse debitResponse = await provider.DebitCustomerAccount(debitRequest);

                CustomerDebit updateCustomerDebit = await _payOutManager.UpdateCustomerDebit(debitResponse, logdebitRequest, providerId);

                if (debitResponse != null && debitResponse.IsSuccessful)
                {
                    await _backgroundService.EnqueueProcess(() => VendBill(requestModel, userDetail, debitResponse, logdebitRequest, merchantRef));
                    response.Succeeded = true;
                    response.Message = "Your request is processing";

                }
                else
                {
                    response.Succeeded = false;
                    response.Message = debitResponse.ResponseMessage;
                }

                return response;

            }
            catch (Exception ex)
            {
                throw new NotSuccessfulException($"An error occured while processing request: {ex}");
            }
        }

        public async Task<VendResponse> VendBill(ClientVendRequest requestModel, Optional<User> userDetail,
            DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest, string merchantRef)
        {
            var provider = _providerSelector.GetProvider(requestModel.Provider);
            var providerId = await provider.GetProviderId(_providerManager);

            if (!debitResponse.ResponseCode.Equals("00"))
            {
                return new VendResponse
                {
                    Succeeded = false,
                    Message = "Failed to debit customer account, insufficient fund."
                };
            }
            ReQueryRequest requeryPayload = new ReQueryRequest
            {
                RetrievalReference = merchantRef,
                Amount = requestModel.Amount
            };

             var requery = await provider.StatusQuery(requeryPayload);
            if (requery != null && !requery.ResponseCode.Equals("00"))
            {
                return new VendResponse
                {
                    Succeeded = false,
                    Message = "Requery failed."
                };
            }

            var logBill = await LogBillPayment(requestModel, providerId, merchantRef);


            var validateRequest = new ValidateRequestModel
            {
                itemCode = requestModel.PaymentCode,
                customerId = requestModel.CustomerId,
                customerPhoneNumber = requestModel.CustomerMobile,
                shouldVerifyCustomer = true,
                customerEmail = userDetail.Value.Email,
                customerName = userDetail.Value.CustomerName,
                amount = requestModel.Amount,
                Provider = Enums.Providers.ZIKORA
            };

            var validate = await Validate(validateRequest);
            if (validate == null || validate.Data == null)
            {
                throw new NotSuccessfulException($"Invalid customerId : {requestModel.CustomerId}.");
            }
            requestModel.validationReference = validate.Data.ValidationRef;
            VendRequest vendRequest = CreateVendRequest(requestModel, merchantRef);


            var vendBill = await _cyberPayProvider.Vend(vendRequest);

            await UpdateBillPayment(vendBill, logBill, providerId);

            return vendBill;
        }


        private VendResponse ValidateRequest(ClientVendRequest requestModel)
        {
            if (string.IsNullOrEmpty(requestModel.CustomerMobile))
            {
                return VendResponseWithMessage("Customer mobile is required");
            }

            if (string.IsNullOrEmpty(requestModel.PaymentCode))
            {
                return VendResponseWithMessage("Payment code is required");
            }

            if (requestModel.Amount < 1)
            {
                return VendResponseWithMessage("Invalid transaction amount");
            }

            if (string.IsNullOrEmpty(requestModel.CustomerId))
            {
                return VendResponseWithMessage("Customer ID is required");
            }

            if (string.IsNullOrEmpty(requestModel.DrAccountNumber))
            {
                return VendResponseWithMessage("Customer Account number is required");
            }

            return new VendResponse { Succeeded = true };
        }

        private async Task<VendResponse> ValidateUserDetail(Optional<User> userDetail, string providerId)
        {
            if (userDetail == null)
            {
                return VendResponseWithMessage("Request from an invalid customer mobile");
            }


            return new VendResponse { Succeeded = true };
        }

        private VendResponse VendResponseWithMessage(string message)
        {
            return new VendResponse
            {
                Message = message,
                Succeeded = false
            };
        }

        private async Task<BillsPayment> LogBillPayment(ClientVendRequest requestModel, string providerId, string merchantRef)
        {
            return await _billsRepository.LogBillPayment(Builder<BillsPayment>.CreateNew()
                .With(u => u.Amount = requestModel.Amount)
                .With(u => u.validationref = requestModel.validationReference)
                .With(u => u.CustomerId = requestModel.CustomerId)
                .With(u => u.ProviderId = providerId)
                .With(u => u.PhoneNumber = requestModel.CustomerMobile)
                .With(u => u.merchantref = merchantRef)
                .With(u => u.CreatedOn = DateTime.Now)
                .With(u => u.UpdatedOn = DateTime.Now)
                .With(u => u.Fee = requestModel.fee)
                .With(u => u.itemcode = requestModel.PaymentCode)
                .Build());
        }

        private VendRequest CreateVendRequest(ClientVendRequest requestModel, string merchantRef)
        {
            return new VendRequest
            {
                amount = requestModel.Amount,
                merchantRef = merchantRef,
                validationRef = requestModel.validationReference
            };
        }

        private async Task UpdateBillPayment(VendResponse vendBill, BillsPayment logBill, string providerId)
        {
            if (vendBill.Succeeded && vendBill.Data != null)
            {
                logBill.processorRef = vendBill.Data.TransactionReference;
                logBill.responsecode = "Successful";
            }
            else
            {
                logBill.responsecode = "Failed";
            }

            await _billsRepository.UpdateBillPayment(logBill, providerId);
        }

    }
}
