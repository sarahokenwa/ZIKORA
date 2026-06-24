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
                Message = result.Message,
                Data = result
            });
        }

        // Initiate PIN reset
        [HttpPost("pin-reset/initiate")]
        public async Task<IActionResult> InitiatePinReset([FromBody] PinResetRequest request)
        {
            var result = await _userManager.InitiatePinReset(request);

            if(result == null)
            {
                return BadRequest(new Response<PinResetResponse?>()
                {
                    Code = ResponseCodes.BadRequest,
                    Succeeded = result?.Success ?? false,
                    Message = result?.Message ?? "Failed to initiate PIN reset. Please check the provided details.",
                    Data = result
                });
            }

            //if(result == null)
            //{
            //    return BadRequest(new { Success = false, Message = "Failed to initiate PIN reset. Please check the provided details." });
            //}

            return Ok(new Response<PinResetResponse?>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result?.Success ?? false,
                Message = result?.Message ?? "Initiated PIN reset successfully.",
                Data = result
            });

            //return Ok(new { result.Message, result.Success});
        }

        // Complete PIN reset
        [HttpPost("pin-reset/complete")]
        public async Task<IActionResult> CompletePinReset([FromBody] CompletePinResetRequest request)
        {
            var result = await _userManager.VerifyOTPAndResetPin(request);

            if (result == null)
            {
                return BadRequest(new Response<PinResetResponse?>()
                {
                    Code = ResponseCodes.BadRequest,
                    Succeeded = result?.Success ?? false,
                    Message = result?.Message ?? "Failed to complete PIN reset. Please check the provided details.",
                    Data = result
                });
            }

            //if (result == null)
            //{
            //    return BadRequest(new { Success = false, Message = "Failed to complete PIN reset. Please check the provided details." });
            //}

            return Ok(new Response<PinResetResponse?>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result?.Success ?? false,
                Message = result?.Message ?? "Completed PIN reset successfully.",
                Data = result
            });
        }
    }
}
