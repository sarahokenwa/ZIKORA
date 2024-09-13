using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class BlockAccountRepository : IBlockAccountRepository
    {

        private readonly DataEntities _dbContext;
        private readonly ILogger<BlockAccountRepository> _log;


        public BlockAccountRepository(DataEntities dbContext, ILogger<BlockAccountRepository> log)
        {
                _dbContext = dbContext;
                _log = log;
        }

        public async Task<BlockAccount> LogBlockAccount(BlockAccount request)
        {
            try
            {
                var blockAccount = await _dbContext.BlockAccounts.AddAsync(request);

                await _dbContext.SaveChangesAsync();

                return blockAccount.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to block account: {JsonConvert.SerializeObject(request)}");
                throw new NotSuccessfulException($"Failed to block account: {ex.Message}");
                
            }
        }

        public async Task<BlockAccount> UpdateBlockAccount(BlockAccount model, string providerId)
        {
            try
            {
                var blockAccount = await _dbContext.BlockAccounts.FirstOrDefaultAsync(u => u.OwnersPhoneNumber == model.OwnersPhoneNumber && u.ProviderId == providerId);

                if (blockAccount != null)
                {
                    //Response description has been assigned in account manager.
                   // blockAccount.ResponseDescription = model.ResponseDescription;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to block account:  {JsonConvert.SerializeObject(model)}");
                throw new NotSuccessfulException($"Failed to block account: {ex.Message}");
            }
        }

    }
}
