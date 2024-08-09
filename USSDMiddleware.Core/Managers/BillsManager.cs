using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Bills;
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

        public BillsManager(IUserRepository userRepository, ICyberPayProvider cyberPayProvider, ILogger<BillsManager> log,
            UssdProviderSelector providerSelector, IProviderManager providerManager, IBillsRepository billsRepository)
        {
            _userRepository = userRepository;
            _cyberPayProvider = cyberPayProvider;
            _providerSelector = providerSelector;
            _log = log;
            _providerManager = providerManager;
            _billsRepository = billsRepository;
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

        public async Task<VendResponse> Vend(ClientVendRequest requestModel)
        {
            var provider = _providerSelector.GetProvider(requestModel.Provider);
            var providerId = await provider.GetProviderId(_providerManager);

            if (string.IsNullOrEmpty(requestModel.CustomerMobile))
            {
                return new VendResponse
                {
                    Message = "Customer mobile is required",
                    Succeeded = false
                };
            }

            if (string.IsNullOrEmpty(requestModel.PaymentCode))
            {
                return new VendResponse
                {
                    Message = "Payment code is required",
                    Succeeded =false
                };
            }

            if (requestModel.Amount < 1)
            {
                return new VendResponse
                {
                    Message = "Invalid transasction amount",
                    Succeeded = false
                };
            }

            if (string.IsNullOrEmpty(requestModel.CustomerId))
            {
                return new VendResponse
                {
                    Message = "Customer ID is required",
                    Succeeded = false
                };
            }
         
            var userDetail = await _userRepository.GetByPhoneNumber(requestModel.CustomerMobile, providerId);

            if (userDetail == null)
            {
                return new VendResponse
                {
                    Message = "Request from an invalid customer mobile",
                    Succeeded = false
                };
            }
            if (!userDetail.Value.TransactionPin.Equals(requestModel.TransactionPin))
            {
                return new VendResponse
                {
                    Message = "Invalid Transaction Pin",
                    Succeeded = false
                };
            }
            string merchantRef = Guid.NewGuid().ToString();

            var logBill = await _billsRepository.LogBillPayment(Builder<BillsPayment>.CreateNew()
                   .With(u => u.Amount = requestModel.Amount)
                   .With(u => u.validationref= requestModel.validationReference)
                   .With(u => u.CustomerId = requestModel.CustomerId)
                   .With(u => u.ProviderId = providerId)
                   .With(u => u.PhoneNumber = requestModel.CustomerMobile)
                   .With(u => u.merchantref =merchantRef)
                   .With(u => u.CreatedOn = DateTime.Now)
                   .With(u => u.UpdatedOn = DateTime.Now)
                   .With(u => u.Fee = requestModel.fee)
                     .With(u => u.itemcode = requestModel.PaymentCode)
                     
                   .Build());
         
            VendRequest request = new VendRequest
            {
               // customerMobile = requestModel.CustomerId,
                //paymentCode = requestModel.PaymentCode,
                amount = requestModel.Amount,
                //customerEmail = "",
                merchantRef = merchantRef,
                validationRef= requestModel.validationReference

            };
            var vendBill= await _cyberPayProvider.Vend(request);

         
            if (vendBill.Succeeded && vendBill.Data != null)
            {
                logBill.processorRef = vendBill.Data.TransactionReference;
                logBill.responsecode = "Successfull";

            }
            else
            {
                logBill.responsecode = "Failed";
            }
            var updateBill = await _billsRepository.UpdateBillPayment(logBill, providerId);
            return vendBill;
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
                customerId= requestModel.customerId,
            customerName= requestModel.customerName,
            email= requestModel.customerEmail,
            itemCode= requestModel.itemCode,
            phone= requestModel.customerPhoneNumber,
            phoneValidation= requestModel.shouldVerifyCustomer,
            };
            return await _cyberPayProvider.Validate(request);
        }

    }
}
