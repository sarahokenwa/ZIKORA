using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface ICardManager
    {
        Task<CardResponse> CardRequest(CardRequest request);
        Task<Card> LogCardRequest(CardRequest request, CardRequestExtension settings, string providerId);
    }
}
