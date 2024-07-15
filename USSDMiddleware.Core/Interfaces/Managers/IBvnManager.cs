using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;

namespace USSDMiddleware.Core.Interfaces.Managers;

public interface IBvnManager
{
    Task<BvnInfoResponse> GetBvnInfo(BvnInfoRequest bvnInfoRequest);
}