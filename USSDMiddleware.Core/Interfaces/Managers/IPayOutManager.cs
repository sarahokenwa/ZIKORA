using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface IPayOutManager
    {
        Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request);
        Task<RequeryResponse> RequeryPayOut(string reference);
        Task<BankResponseDto[]> Get();
        Task<CustomerDebit> LogCustomerDebit(InstantPayOutRequest request, ZIKORAModelExtension settings, string providerId);
        Task<CustomerDebit> UpdateCustomerDebit(DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest, string providerId);
        Task<FundTransfer> LogInstantPayment(InstantPayOutRequest request, string merchantReference, string providerId);
    }
}
