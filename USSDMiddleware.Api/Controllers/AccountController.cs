using Microsoft.AspNetCore.Mvc;
using static USSDMiddleware.Core.Constants;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using USSDMiddleware.Core.Models.Accounts;

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

        //For existing ZIKORA Customers not registered for USSD
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewAccount([FromBody] CreateAccountRequestExtension request)
        {
            var result = await _accountManager.CreateAccount(request);

            return Ok(new Response<CreateAccountResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("name-enquiry")]
        public async Task<IActionResult> NameEnquiry([FromBody] NameEnquiryRequest request)
        {
            var result = await _accountManager.NameEnquiry(request);

            return Ok(new Response<NameEnquiryResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpGet("validate-account")]
        public async Task<IActionResult> GetUserByAccountNumber([FromQuery] AccountValidationRequest request)
        {
            var result = await _accountManager.GetUserByAccountNumber(request);

            return Ok(new Response<GetUserByAccountNumberResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("block-account")]
        public async Task<IActionResult> BlockAccount([FromBody] BlockAccountRequest request)
        {
            var result = await _accountManager.BlockAccount(request);

            return Ok(new Response<BlockAccountResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("deactivate-pnd")]
        public async Task<IActionResult> DeactivatePND([FromBody] BlockAccountRequest request)
        {
            var result = await _accountManager.DeactivatePND(request);

            return Ok(new Response<BlockAccountResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("verify-pnd-status")]
        public async Task<IActionResult> VerifyPNDStatus([FromBody] BlockAccountRequest request)
        {
            var result = await _accountManager.VerifyPNDStatus(request);

            return Ok(new Response<BlockAccountResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }
    }
}
