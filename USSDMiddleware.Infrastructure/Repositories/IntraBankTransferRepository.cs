using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class IntraBankTransferRepository : IIntraBankTransferRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<IntraBankTransferRepository> _log;

        public IntraBankTransferRepository(DataEntities dbContext, ILogger<IntraBankTransferRepository> log)
        {
            _dbContext = dbContext;
            _log = log;
        }
        public async Task<IntraBankTransfer> LogIntraBankTransfer(IntraBankTransfer intraBankTransfer)
        {
            try
            {
                var localTransfer = await _dbContext.IntraBankTransfers.AddAsync(intraBankTransfer);

                await _dbContext.SaveChangesAsync();

                return localTransfer.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to save intra bank transfer: {JsonConvert.SerializeObject(intraBankTransfer)}");
                throw new NotSuccessfulException($"Failed to save intra bank transfer: {ex.Message}");
            }
        }

        public async Task<IntraBankTransfer> UpdateIntraBankTransfer(IntraBankTransfer model, string providerId)
        {
            try
            {
                var localTransfer = await _dbContext.IntraBankTransfers.FirstOrDefaultAsync(u => u.RetrievalReference == model.RetrievalReference && u.ProviderId == providerId);

                if (localTransfer != null)
                {
                    localTransfer.ProcessorRef = model.RetrievalReference;
                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to update intra bank transfer:  {JsonConvert.SerializeObject(model)}");
                throw new NotSuccessfulException($"Failed to update intra bank : {ex.Message}");
                ;
            }
        }

    }
}
