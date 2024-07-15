using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USSDMiddleware.Core;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;

namespace USSDMiddleware.Api.Controllers;

[Route("api/v1/bvn")]
[ApiController]
[Authorize]
public class BvnController : ControllerBase
{
    private readonly IBvnManager _bvnManager;
        
    public BvnController(IBvnManager bvnManager)
    {
        _bvnManager = bvnManager;  
    }

    [HttpPost("info")]
    public async Task<IActionResult> GetBvnInfo([FromBody] BvnInfoRequest request)
    {
        var result = await _bvnManager.GetBvnInfo(request);

        return Ok(new Response<BvnInfoResponse>()
        {
            Code = Constants.ResponseCodes.Successful,
            Succeeded = true,
            Data = result
        });
    }
}