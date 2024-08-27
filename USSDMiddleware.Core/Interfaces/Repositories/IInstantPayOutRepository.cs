using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IInstantPayOutRepository
    {
        Task<FundTransfer> LogInstantPayment(FundTransfer fundTransfer);
        Task<FundTransfer> UpdateInstantPayment(FundTransfer model, string providerId);
    }
}
