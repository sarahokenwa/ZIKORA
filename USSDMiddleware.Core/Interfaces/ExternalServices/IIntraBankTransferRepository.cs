using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.ExternalServices
{
    public interface IIntraBankTransferRepository
    {
        Task<IntraBankTransfer> LogIntraBankTransfer(IntraBankTransfer intraBankTransfer);
        Task<IntraBankTransfer> UpdateIntraBankTransfer(IntraBankTransfer model, string providerId);
    }
}
