using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Providers
{
    public interface IUssdProvider
    {
        Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request);
        Task<GetUserByPhoneNumberResponse> GetUserByPhoneNumber(string phoneNumber);
        Enums.Providers ProviderType { get; }
        Task<AccountCreationResponse> CreateAccount(AccountCreationRequest request);

        Task<BvnInfoResponse> GetBvnInfo(string bvn, string phoneNo);
        Task<string> GetProviderId(IProviderManager providerManager);
        Task<BalanceEnquiryResponse> CheckAccountBalance(BalanceEnquiryRequest model);
        Task<List<GetAccountResponse>> GetAccountsByPhoneNumber(string phoneNumber);
        Task<GetUserByAccountNumberResponse> GetUserByAccountNumber(string accountNumber);
        Task<DebitCustomerAccountResponse> DebitCustomerAccount(DebitCustomerAccountRequest model);
        Task<CardResponse> CardRequest(CardRequestExtension request);
        Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request);
        Task<BlockAccountResponse> VerifyPNDStatus(BlockAccountRequest request);
        Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request);
        Task<GetCustomerCardResponse> GetCustomerCards(GetCustomerCardRequest request);
        Task<FreezeCardResponse> FreezeCard(FreezeCardRequest request);
        Task<UnFreezeCardResponse> UnFreezeCard(UnFreezeCardRequest request);
    }
}
