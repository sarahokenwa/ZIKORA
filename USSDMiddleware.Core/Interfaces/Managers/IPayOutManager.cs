using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface IPayOutManager
    {
        //Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request);
        Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequestExtension request); 
        Task<IntraBankTransferResponse> IntraBankTransfer(IntraBankTransferRequestExtension request);
        Task<RequeryResponse> RequeryPayOut(string reference);
        Task<BankResponseDto[]> Get();
        Task<CustomerDebit> LogCustomerDebit(CustomerDebit request);
        Task<CustomerDebit> UpdateCustomerDebit(DebitCustomerAccountResponse debitResponse, CustomerDebit logdebitRequest, string providerId);
        Task<FundTransfer> LogInstantPayment(InstantPayOutRequestExtension request, string merchantReference, string providerId);
        Task<IntraBankTransfer> LogIntraBankTransfer(IntraBankTransfer request);
        Task<IntraBankTransfer> UpdateIntraBankTransfer(IntraBankTransferResponse intraBankTransferResponse, IntraBankTransfer logIntraBankTransferRequest, string providerId);
    }
}
