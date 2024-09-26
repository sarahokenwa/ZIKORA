using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models;

namespace USSDMiddleware.Core.Utilities;

public class BuildUtil
{
    private readonly IConfiguration _configuration;

    public BuildUtil(IConfiguration configuration)
    {
       _configuration = configuration;
    }

    public static AccountCreationRequest BuildAccountCreationRequest(ValidationLog validationLog, IConfiguration configuration)
    {
        var accountName = $"{validationLog.LastName}{validationLog.FirstName}";
        return Builder<AccountCreationRequest>.CreateNew()
            .With(a => a.AccountOpeningTrackingRef = Guid.NewGuid().ToString())
            .With(a => a.TransactionTrackingRef = Guid.NewGuid().ToString())
            .With(a => a.ProductCode = configuration["ApiOptions:Zikora:ProductCode"])
            .With(a => a.LastName = validationLog.LastName)
            .With(a => a.OtherNames = validationLog.OtherNames)
            .With(a => a.BVN = validationLog.Bvn)
            .With(a => a.AccountName = accountName)
            .With(a => a.PhoneNo = validationLog.PhoneNumber)
            .With(a => a.Gender = 0)
            .With(a => a.DateOfBirth = validationLog.Dob)
            .With(a => a.AccountOfficerCode = configuration["ApiOptions:Zikora:ProductCode"])
            .With(a => a.Email = "")
            .With(a => a.AccountTier = "1")
            .Build();
    }
}