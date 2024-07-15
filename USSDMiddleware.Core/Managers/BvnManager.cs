using FizzWare.NBuilder;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;

namespace USSDMiddleware.Core.Managers;

public class BvnManager : IBvnManager
{
    private readonly UssdProviderSelector _providerSelector;

    public BvnManager(UssdProviderSelector providerSelector)
    {
        _providerSelector = providerSelector;
    }

    public Task<BvnInfoResponse> GetBvnInfo(BvnInfoRequest req)
    {
        ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
            .With(v => v.PhoneNumber = req.PhoneNumber)
            .With(v => v.Bvn = req.Bvn)
            .Build());

        return _providerSelector.GetProvider(req.Provider).GetBvnInfo(req.Bvn, req.PhoneNumber);
    }
}