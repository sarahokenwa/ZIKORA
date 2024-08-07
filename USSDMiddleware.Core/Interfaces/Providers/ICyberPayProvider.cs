using USSDMiddleware.Core.Models.Bills;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Providers
{
    public interface ICyberPayProvider
    {
        Task<CategoriesResponse> GetCategories(string categoryType);
        Task<BillersResponse> GetBillers(string categoryId);
        Task<PaymentItemsResponse> GetPaymentItems(string billerId);
        Task<VendResponse> Vend(VendRequest requestModel);
        Task<ValidateResponse> Validate(ValidateRequest requestModel);
        Task<CyberPayPayoutAuthResponse> GetClientCredentials();
    }
}
