using Microsoft.AspNetCore.Mvc;
using static USSDMiddleware.Core.Constants;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;

namespace USSDMiddleware.Api.Controllers
{
    [Route("api/v1/payout")]
    [ApiController]
    [Authorize]
    public class PayOutController : ControllerBase
    {
        private readonly IPayOutManager _payOutManager;
        public PayOutController(IPayOutManager payOutManager)
        {
            _payOutManager = payOutManager;
        }

        [HttpPost("instant")]
        public async Task<IActionResult> InstantPayOutTransfer([FromBody] InstantPayOutRequest request)
        {
            var result = await _payOutManager.InstantPayOut(request);

            return Ok(new Response<InstantPayOutResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpGet("requery/{reference}")]
        public async Task<IActionResult> RequeryPayOutTransfer(string reference)
        {
            var result = await _payOutManager.RequeryPayOut(reference);

            return Ok(new Response<RequeryResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = result
            });
        }

        [HttpGet("banks")]
        public async Task<IActionResult> GetBanks()
        {
            var bankResponse = await _payOutManager.Get();

            if (bankResponse == null || bankResponse.Data == null || !bankResponse.Data.Any())
            {
                return NotFound(new Response<BankResponse>
                {
                    Code = ResponseCodes.NotFound,
                    Succeeded = false,
                    Data = null
                });
            }

            return Ok(new Response<BankResponse>
            {
                Code = ResponseCodes.Successful,
                Succeeded = true,
                Data = bankResponse
            });
        }
    }
}


