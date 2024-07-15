using USSDMiddleware.Core.Models.Bills;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface IBillsManager
    {
        Task<CategoriesResponse> GetCategories(string categoryType);
        Task<BillersResponse> GetBillers(string categoryId);
        Task<PaymentItemsResponse> GetPaymentItems(string billerId);
        Task<VendResponse> Vend(ClientVendRequest requestModel);
    }
}
