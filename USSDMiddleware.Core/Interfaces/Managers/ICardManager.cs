using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using Card = USSDMiddleware.Core.Entities.Card;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface ICardManager
    {
        Task<CardResponse> CardRequest(CardRequest request);
        Task<Card> LogCardRequest(CardRequest request, CardRequestExtension settings, string providerId);
        Task<FreezeCardResponse> FreezeCard(FreezeCardRequest request);
        Task<UnFreezeCardResponse> UnFreezeCard(UnFreezeCardRequest request);
    }
}
