using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using USSDMiddleware.Core.Entities;
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
                _log.LogError(ex, "Failed to save instant payment: {SerializeObject}", JsonConvert.SerializeObject(fundTransfer));
                throw;
            }
        }

        public async Task<FundTransfer> UpdateInstantPayment(FundTransfer model, string providerId)
        {
            try
            {
                var instantPayment = await _dbContext.FundTransfers.FirstOrDefaultAsync(u => u.MerchantRef == model.MerchantRef && u.ProviderId == providerId);

                if (instantPayment != null)
                {
                    
                   // instantPayment.requeryresponsecode = model.requeryresponsecode;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to update instant payment: {SerializeObject}", JsonConvert.SerializeObject(model));
                throw;
            }
        }

    }
}
