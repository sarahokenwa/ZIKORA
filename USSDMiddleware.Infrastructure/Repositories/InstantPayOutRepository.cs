using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class InstantPayOutRepository : IInstantPayOutRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<InstantPayOutRepository> _log;

        public InstantPayOutRepository(DataEntities dbContext, ILogger<InstantPayOutRepository> log)
        {
            _dbContext = dbContext;
            _log = log;
        }

        public async Task<FundTransfer> LogInstantPayment(FundTransfer fundTransfer)
        {
            try
            {
                var instantPayment = await _dbContext.FundTransfers.AddAsync(fundTransfer);

                await _dbContext.SaveChangesAsync();

                return instantPayment.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to save instant payment: {fundTransfer}", JsonConvert.SerializeObject(fundTransfer));
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<FundTransfer> UpdateInstantPayment(FundTransfer model, string providerId)
        {
            try
            {
                var instantPayment = await _dbContext.FundTransfers.FirstOrDefaultAsync(u => u.MerchantRef == model.MerchantRef && u.ProviderId == providerId);

                if (instantPayment != null)
                {
                    instantPayment.Code = model.Code;
                    instantPayment.Succeeded = model.Succeeded;
                    instantPayment.SessionId = model.SessionId;
                    instantPayment.Message = model.Message;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to update instant payment: {model}", JsonConvert.SerializeObject(model));
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<decimal> GetCumulativeFundTransferToday(string senderAccountNumber)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var totalAmount = await _dbContext.FundTransfers
                    .Where(ft => ft.SenderAccountNumber.Equals(senderAccountNumber) && ft.CreatedOn.Date == today && ft.Succeeded)
                    .SumAsync(ft => ft.Amount);

                return totalAmount;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to get cumulative fund transfer for today for account: {senderAccountNumber}");
                throw new NotSuccessfulException(ex.Message);
            }
        }
    }
}
