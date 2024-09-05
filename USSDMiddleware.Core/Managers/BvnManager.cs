using FizzWare.NBuilder;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;

namespace USSDMiddleware.Core.Managers;

public class BvnManager : IBvnManager
{
    private readonly UssdProviderSelector _providerSelector;
    private readonly IValidationLogManager _validationLogManager;

    public BvnManager(UssdProviderSelector providerSelector, IValidationLogManager validationLogManager)
    {
        _providerSelector = providerSelector;
        _validationLogManager = validationLogManager;
    }

    public async Task<GetBvnInfoResponse> GetBvnInfo(BvnInfoRequest req)
    {
        ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
            .With(v => v.PhoneNumber = req.PhoneNumber)
            .With(v => v.Bvn = req.Bvn)
            .Build());

        BvnInfoResponse bvnInfoResponse =
            await _providerSelector.GetProvider(req.Provider).GetBvnInfo(req.Bvn, req.PhoneNumber);

        if (!bvnInfoResponse.isBvnValid)
        {
            return new GetBvnInfoResponse("", bvnInfoResponse.isBvnValid, "");
        }

        if(string.IsNullOrEmpty(bvnInfoResponse.bvnDetails.Email))
        {
            bvnInfoResponse.bvnDetails.Email = RandomEmailGenerator.GenerateRandomEmail();
        }

        var createdValidationLog = await _validationLogManager.CreateValidationLog(Builder<ValidationLog>.CreateNew()
          .With(v => v.Bvn = bvnInfoResponse.bvnDetails.BVN)
           // .With(v => v.PhoneNumber = bvnInfoResponse.bvnDetails.phoneNumber)
            .With(v => v.PhoneNumber = req.PhoneNumber)
            .With(v => v.FirstName = bvnInfoResponse.bvnDetails.FirstName)
            .With(v => v.LastName = bvnInfoResponse.bvnDetails.LastName)
            .With(v => v.Dob = bvnInfoResponse.bvnDetails.DOB)
            .With(v => v.Email = bvnInfoResponse.bvnDetails.Email)
            .With(v => v.OtherNames = bvnInfoResponse.bvnDetails.OtherNames)
            .With(v => v.Valid = true)
            .Build());

        return new GetBvnInfoResponse(createdValidationLog.Id, bvnInfoResponse.isBvnValid, createdValidationLog.Dob);
    }

}