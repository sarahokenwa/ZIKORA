using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Bills;

namespace USSDMiddleware.Core.Managers
{
    public class BillsManager : IBillsManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ICyberPayProvider _cyberPayProvider;
        private readonly ILogger<BillsManager> _log;

        public BillsManager(IUserRepository userRepository, ICyberPayProvider cyberPayProvider, ILogger<BillsManager> log)
        {
            _userRepository = userRepository;
            _cyberPayProvider = cyberPayProvider;
            _log = log;
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
            if (string.IsNullOrEmpty(requestModel.CustomerMobile))
            {
                return new VendResponse
                {
                    Message = "Customer mobile is required",
                    ResponseCode = "96"
                };
            }

            if (string.IsNullOrEmpty(requestModel.PaymentCode))
            {
                return new VendResponse
                {
                    Message = "Payment code is required",
                    ResponseCode = "96"
                };
            }

            if (requestModel.Amount < 1)
            {
                return new VendResponse
                {
                    Message = "Invalid transasction amount",
                    ResponseCode = "96"
                };
            }

            if (string.IsNullOrEmpty(requestModel.CustomerId))
            {
                return new VendResponse
                {
                    Message = "Customer ID is required",
                    ResponseCode = "96"
                };
            }
            var userDetail = await _userRepository.GetByPhoneNumber(requestModel.CustomerMobile);

            if (userDetail == null)
            {
                return new VendResponse
                {
                    Message = "Request from an invalid customer mobile",
                    ResponseCode = "96"
                };
            }

            VendRequest request = new VendRequest
            {
                customerMobile = requestModel.CustomerMobile,
                paymentCode = requestModel.PaymentCode,
                amount = requestModel.Amount,
                customerEmail = ""
            };
            return await _cyberPayProvider.Vend(request);
        }
    }
}
