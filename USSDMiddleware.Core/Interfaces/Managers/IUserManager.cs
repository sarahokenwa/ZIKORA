using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface IUserManager
    {
        Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request);
        Task<CreateUserResponse> CreateUser(CreateUserRequest request);
        Task<UserPhoneNumberDetails> GetUserByPhoneNumber(PhoneValidationRequest request);
        Task<List<UserAccountNumber>> GetAccountsByPhoneNumber(PhoneValidationRequest request);
        Task<AccountBalanceEnquiry> GetAccountBalance(AccountRequest request);
    }
}
