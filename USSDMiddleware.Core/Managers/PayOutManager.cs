using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Managers
{
    public class PayOutManager : IPayOutManager
    {
        private readonly IPayOutService _payOutService;
        private readonly ILogger<PayOutManager> _log;

        public PayOutManager(IPayOutService payOutService, ILogger<PayOutManager> log)
        {
            _payOutService = payOutService; 
            _log = log;
        }

        public async Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request)
        {
            try
            {
                var instantPayOut = await _payOutService.InstantPayOut(request);

                return instantPayOut;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to make instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Instant payout failed.");
            }
        }

        public async Task<RequeryResponse> RequeryPayOut(string reference)
        {
            try
            {
                var requeryResponse = await _payOutService.RequeryPayOut(reference);

                return requeryResponse;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to requery instant payment.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Requery failed.");
            }
        }

        public async Task<BankResponseDto[]> Get()
        {
            try
            {
                var bankResponse = await _payOutService.Get();

                if (bankResponse == null || bankResponse.Data == null || !bankResponse.Data.Any())
                {
                    _log.LogError("Failed to retrieve bank data.");
                    throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Failed to retrieve bank data.");
                }

                var bankList = bankResponse.Data.Select(b => new BankResponseDto
                {
                    Id = b.Id,
                    BankCode = b.BankCode,
                    BankName = b.BankName
                }).ToArray();
                return bankList;

                //return new BankResponse
                //{
                //    Code = bankResponse.Code,
                //    Succeeded = bankResponse.Succeeded,
                //    Data = bankList
                //};
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to retrieve banks.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Get all Banks failed.");
            }
        }


    }
}
