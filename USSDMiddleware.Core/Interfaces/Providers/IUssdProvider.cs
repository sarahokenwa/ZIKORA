using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Providers
{
    public interface IUssdProvider
    {
         Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request);
         Enums.Providers ProviderType { get; }
         Task<AccountCreationResponse> CreateAccount(AccountCreationRequest request);

         Task<BvnInfoResponse> GetBvnInfo(string bvn, string phoneNo);
    }
}
