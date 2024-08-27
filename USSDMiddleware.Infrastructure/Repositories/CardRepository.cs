using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<CardRepository> _log;

        public CardRepository(DataEntities dbContext, ILogger<CardRepository> log) 
        { 
            _dbContext = dbContext;
            _log = log;
        }

        public async Task<Card> LogCardRequest(Card card)
        {
            try
            {
                var cardRequest = await _dbContext.Cards.AddAsync(card);

                await _dbContext.SaveChangesAsync();

                return cardRequest.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to make card request: {card}", JsonConvert.SerializeObject(card), ex);
                throw new NotSuccessfulException(ex.Message);
            }
        }

       public async Task<Card> UpdateCardRequest(Card model, string providerId)
        {
            try
            {
                var updateCardRequest = await _dbContext.Cards.FirstOrDefaultAsync(u => u.AccountNumber == model.AccountNumber && u.ProviderId == providerId);

                if (updateCardRequest != null)
                {
                    updateCardRequest.BatchNo = model.BatchNo;
                    updateCardRequest.IsSuccessful = model.IsSuccessful;
                    updateCardRequest.Identifier  = model.Identifier;
                    updateCardRequest.ResponseMessage = model.ResponseMessage;
                    updateCardRequest.ProviderId = providerId;

                    await _dbContext.SaveChangesAsync();
                }

                return model;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to make card request: {model}", JsonConvert.SerializeObject(model));
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<Card> LogCardResponse(Card card)
        {
            try
            {
                var cardResponse = await _dbContext.Cards.AddAsync(card);

                await _dbContext.SaveChangesAsync();

                return cardResponse.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to update card response: {card}", JsonConvert.SerializeObject(card));
                throw new NotSuccessfulException(ex.Message);
            }
        }
    }
}
