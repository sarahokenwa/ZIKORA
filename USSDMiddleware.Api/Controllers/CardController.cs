using Microsoft.AspNetCore.Mvc;
using USSDMiddleware.Core.Interfaces.Managers;
using static USSDMiddleware.Core.Constants;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Request;
using Microsoft.AspNetCore.Authorization;

namespace USSDMiddleware.Api.Controllers
{

    [Route("api/v1/card")]
    [ApiController]
    [Authorize]
    public class CardController : ControllerBase
    { 
        private readonly ICardManager _cardManager; 


        public CardController(ICardManager cardManager)
        {
            _cardManager = cardManager;
        }

        [HttpPost("card-request")]
        public async Task<IActionResult> CardRequest([FromBody] CardRequest request)
        {
            var result = await _cardManager.CardRequest(request);
            return Ok(new Response<CardResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result.IsSuccessful,
                Message = result.ResponseMessage,
                Data = result
            });
        }

        [HttpPost("freeze-card")]
        public async Task<IActionResult> FreezeCard([FromBody] FreezeCardRequest request)
        {
            var result = await _cardManager.FreezeCard(request);
            return Ok(new Response<FreezeCardResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result.IsSuccessful,
                Message = result.ResponseMessage,
                Data = result
            });
        }

        [HttpPost("unfreeze-card")]
        public async Task<IActionResult> UnFreezeCard([FromBody] UnFreezeCardRequest request)
        {
            var result = await _cardManager.UnFreezeCard(request);
            return Ok(new Response<UnFreezeCardResponse>()
            {
                Code = ResponseCodes.Successful,
                Succeeded = result.IsSuccessful,
                Message = result.ResponseMessage,
                Data = result
            });
        }

    }
}
