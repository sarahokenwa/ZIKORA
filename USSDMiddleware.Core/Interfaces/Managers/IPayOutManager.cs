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
    }
}
