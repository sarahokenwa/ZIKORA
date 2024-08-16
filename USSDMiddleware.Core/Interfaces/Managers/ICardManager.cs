using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers
{
    public interface ICardManager
    {
        Task<CardResponse> CardRequest(CardRequest request);
    }
}
