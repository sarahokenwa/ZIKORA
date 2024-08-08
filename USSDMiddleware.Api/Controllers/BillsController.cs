using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Models.Bills;

namespace USSDMiddleware.Api.Controllers;

[Authorize]
[Route("api/v1/bills")]
[ApiController]
public class BillsController : ControllerBase
{

    private readonly IBillsManager _billsManager;

    public BillsController(IBillsManager billsManager)
    {
        _billsManager = billsManager;
    }

    [HttpPost("vend")]
    public async Task<IActionResult> Vend([FromBody] ClientVendRequest request)
    {
        var result = await _billsManager.Vend(request);

        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(string categoryType)
    {
        var result = await _billsManager.GetCategories(categoryType);

        return Ok(result);
    }

    [HttpGet("billers")]
    public async Task<IActionResult> GetBillers(string categoryId)
    {
        var result = await _billsManager.GetBillers(categoryId);

        return Ok(result);
    }


    [HttpGet("payment-items")]
    public async Task<IActionResult> GetPaymentItem(string billerId)
    {
        var result = await _billsManager.GetPaymentItems(billerId);

        return Ok(result);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateRequestModel request)
    {
        var result = await _billsManager.Validate(request);

        return Ok(result);
    }
}