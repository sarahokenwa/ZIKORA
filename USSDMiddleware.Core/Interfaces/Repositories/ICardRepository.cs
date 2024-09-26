using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface ICardRepository
    {
        Task<Card> LogCardRequest(Card card);
        Task<Card> UpdateCardRequest(Card model, string providerId);
        Task<Card> LogCardResponse(Card card);
    }
}
