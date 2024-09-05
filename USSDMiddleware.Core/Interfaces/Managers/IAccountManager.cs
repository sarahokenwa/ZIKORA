using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface IAccountManager
    {
        Task<CreateAccountResponse> CreateAccount(CreateAccountRequest newUser);
        Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest request);
        Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request);
        Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request);
        Task<BlockAccountResponse> VerifyPNDStatus(BlockAccountRequest request);
    }
}
