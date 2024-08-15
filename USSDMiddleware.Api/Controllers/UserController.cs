using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using static USSDMiddleware.Core.Constants;

namespace USSDMiddleware.Api.Controllers
{
    [Route("api/v1/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IUserManager _userManager;

        public UserController(IUserManager userManager)
        {
            _userManager = userManager;

        }
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _userManager.CreateUser(request);

            return Ok(new Response<CreateUserResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("validate-phone")]
        public async Task<IActionResult> ValidatePhone([FromBody] PhoneValidationRequest request)
        {
            var result = await _userManager.ValidatePhone(request);

            return Ok(new Response<PhoneValidationResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }


        [HttpPost("validate-dob")]
        public async Task<IActionResult> GetUserByPhoneNumber([FromBody] PhoneValidationRequest request)
        {
            var result = await _userManager.GetUserByPhoneNumber(request);

            return Ok(new Response<UserPhoneNumberDetails>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> GetAccountsByPhoneNumber([FromBody] PhoneValidationRequest request)
        {
            var result = await _userManager.GetAccountsByPhoneNumber(request);

            return Ok(new Response<List<UserAccountNumber>>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpPost("account-balance")]
        public async Task<IActionResult> GetAccountBalance([FromBody] AccountRequest request)
        {
            var result = await _userManager.GetAccountBalance(request);

            return Ok(new Response<AccountBalanceEnquiry>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result.Status,
                Message= result.Message,
                Data = result
            });
        }
    }
}
