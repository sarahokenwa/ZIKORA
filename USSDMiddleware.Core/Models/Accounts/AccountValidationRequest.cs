using Microsoft.AspNetCore.Mvc;

namespace USSDMiddleware.Core.Models.Accounts
{
    public class AccountValidationRequest
    {
        [FromQuery(Name = "accountNumber")]
        public string AccountNumber { get; set; }

        [FromQuery(Name = "provider")]
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

    }
}
