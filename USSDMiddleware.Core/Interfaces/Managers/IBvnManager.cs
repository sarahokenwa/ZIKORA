using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Interfaces.Managers;

public interface IBvnManager
{
    Task<GetBvnInfoResponse> GetBvnInfo(BvnInfoRequest bvnInfoRequest);
}