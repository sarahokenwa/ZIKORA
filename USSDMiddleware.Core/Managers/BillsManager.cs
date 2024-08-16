using Aornis;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Bills;
using USSDMiddleware.Core.Models.Request;
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

        //public async Task<VendResponse> Vend(ClientVendRequest requestModel)
        //{
        //    var provider = _providerSelector.GetProvider(requestModel.Provider);
        //    var providerId = await provider.GetProviderId(_providerManager);
        //    if (string.IsNullOrEmpty(requestModel.CustomerMobile))
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Customer mobile is required",
        //            Succeeded = false
        //        };
        //    }

        //    if (string.IsNullOrEmpty(requestModel.PaymentCode))
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Payment code is required",
        //            Succeeded = false
        //        };
        //    }

        //    if (requestModel.Amount < 1)
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Invalid transasction amount",
        //            Succeeded = false
        //        };
        //    }

        //    if (string.IsNullOrEmpty(requestModel.CustomerId))
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Customer ID is required",
        //            Succeeded = false
        //        };
        //    }
        //    if (string.IsNullOrEmpty(requestModel.DrAccountNumber))
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Customer Account number is required",
        //            Succeeded = false
        //        };
        //    }
        //    var userDetail = await _userRepository.GetByPhoneNumber(requestModel.CustomerMobile, providerId);

        //    if (userDetail == null)
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Request from an invalid customer mobile",
        //            Succeeded = false
        //        };
        //    }
        //    if (!userDetail.Value.TransactionPin.Equals(requestModel.TransactionPin))
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Invalid Transaction Pin",
        //            Succeeded = false
        //        };
        //    }

        //    string merchantRef = Guid.NewGuid().ToString();


        //    var logBill = await _billsRepository.LogBillPayment(Builder<BillsPayment>.CreateNew()
        //           .With(u => u.Amount = requestModel.Amount)
        //           .With(u => u.validationref = requestModel.validationReference)
        //           .With(u => u.CustomerId = requestModel.CustomerId)
        //           .With(u => u.ProviderId = providerId)
        //           .With(u => u.PhoneNumber = requestModel.CustomerMobile)
        //           .With(u => u.merchantref = merchantRef)
        //           .With(u => u.CreatedOn = DateTime.Now)
        //           .With(u => u.UpdatedOn = DateTime.Now)
        //           .With(u => u.Fee = requestModel.fee)
        //             .With(u => u.itemcode = requestModel.PaymentCode)
        //           .Build());
        //    //debit zikora customers account before proceeding
        //   /** var debitCustomer = await provider.DebitCustomerAccount(new DebitCustomerAccountRequest
        //    {
        //        AccountNumber = requestModel.DrAccountNumber,
        //        Amount = requestModel.Amount,
        //        Fee = requestModel.fee,
        //        Narration = "Bills/Airtime",
        //        GLCode = "",
        //        NibssCode = "",
        //        RetrievalReference = merchantRef
        //    });

        //    if (debitCustomer == null)
        //    {
        //        return new VendResponse
        //        {
        //            Message = "Could not debit account",
        //            Succeeded = false
        //        };
        //    }**/
        //    VendRequest request = new VendRequest
        //    {
        //        amount = requestModel.Amount,
        //        merchantRef = merchantRef,
        //        validationRef = requestModel.validationReference
        //    };
        //    var vendBill = await _cyberPayProvider.Vend(request);


        //    if (vendBill.Succeeded && vendBill.Data != null)
        //    {
        //        logBill.processorRef = vendBill.Data.TransactionReference;
        //        logBill.responsecode = "Successfull";

        //    }
        //    else
        //    {
        //        logBill.responsecode = "Failed";
        //    }
        //    await _billsRepository.UpdateBillPayment(logBill, providerId);

        //    return vendBill;



        //}



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
            var validationResult = ValidateRequest(requestModel);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            var provider = _providerSelector.GetProvider(requestModel.Provider);
            var providerId = await provider.GetProviderId(_providerManager);
            var userDetail = await _userRepository.GetByPhoneNumber(requestModel.CustomerMobile, providerId);
            var userValidationResult = ValidateUserDetail(userDetail, requestModel);
            if (!userValidationResult.Succeeded)
            {
                return userValidationResult;
            }
            string merchantRef = Guid.NewGuid().ToString();

            //log debit customer request, debit customer, update debit customer response
            //if debit customer is successfull, perform line 255 to 260

            var logBill = await LogBillPayment(requestModel, providerId, merchantRef);
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

        private VendResponse ValidateUserDetail(Optional<User> userDetail, ClientVendRequest requestModel)
        {
            if (userDetail == null)
            {
                return VendResponseWithMessage("Request from an invalid customer mobile");
            }

            if (!userDetail.Value.TransactionPin.Equals(requestModel.TransactionPin))
            {
                return VendResponseWithMessage("Invalid Transaction Pin");
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
