using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.ExternalServices
{
    public interface IPayOutService
    {
        Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest request);
        Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request);
        Task<RequeryResponse> RequeryPayOut(string reference);
        Task<BankResponse> Get();
    }
}
