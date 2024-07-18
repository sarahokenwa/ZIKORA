using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
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
    }
}
