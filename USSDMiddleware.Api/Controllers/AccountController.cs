using Microsoft.AspNetCore.Mvc;
using static USSDMiddleware.Core.Constants;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;

namespace USSDMiddleware.Api.Controllers
{
    [Route("api/v1/account")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;
        
        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;  
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewAccount([FromBody] CreateAccountRequest request)
        {
            var result = await _accountManager.CreateAccount(request);

            return Ok(new Response<CreateAccountResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }
    }
}
