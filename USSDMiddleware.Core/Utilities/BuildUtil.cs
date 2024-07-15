using FizzWare.NBuilder;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models;

namespace USSDMiddleware.Core.Utilities;

public class BuildUtil
{
    public static AccountCreationRequest BuildAccountCreationRequest(ValidationLog validationLog)
    {
        var accountName = $"{validationLog.LastName}{validationLog.FirstName}";
        return Builder<AccountCreationRequest>.CreateNew()
            .With(a => a.AccountOpeningTrackingRef = "")
            .With(a => a.TransactionTrackingRef = "")
            .With(a => a.ProductCode = "")
            .With(a => a.LastName = validationLog.LastName)
            .With(a => a.OtherNames = validationLog.OtherNames)
            .With(a => a.BVN = validationLog.Bvn)
            .With(a => a.AccountName = accountName)
            .With(a => a.PhoneNo = validationLog.PhoneNumber)
            .With(a => a.Gender = 0)
            .With(a => a.DateOfBirth = validationLog.Dob)
            .With(a => a.AccountOfficerCode = "")
            .With(a => a.Email = "")
            .With(a => a.AccountTier = "")
            .Build();
    }
}