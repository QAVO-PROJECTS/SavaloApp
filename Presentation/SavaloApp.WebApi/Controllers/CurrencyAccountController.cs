using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CurrencyAccount;
using SavaloApp.Application.GlobalException;
using System.Security.Claims;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/currency-accounts")]
[Authorize]
public class CurrencyAccountController : ControllerBase
{
    private readonly ICurrencyAccountService _service;
    private readonly ILogger<CurrencyAccountController> _logger;

    public CurrencyAccountController(
        ICurrencyAccountService service,
        ILogger<CurrencyAccountController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(async () =>
        {
            var userId = GetUserId();

            var result = await _service.GetAllCurrenciesAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "CURRENCY_LIST_SUCCESS",
                data = result
            });
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();

            var result = await _service.GetCurrencyAsync(id, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "CURRENCY_GET_SUCCESS",
                data = result
            });
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCurrencyAccountDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();

            var result = await _service.CreateCurrencyAsync(dto, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "CURRENCY_CREATE_SUCCESS",
                data = result
            });
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateCurrencyAccountDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();

            await _service.UpdateCurrencyAsync(dto, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "CURRENCY_UPDATE_SUCCESS"
            });
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();

            await _service.DeleteCurrencyAsync(id, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "CURRENCY_DELETE_SUCCESS"
            });
        });
    }

    // 🔐 USER ID FROM TOKEN
    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    }

    // 🔁 ERROR HANDLER
    private async Task<IActionResult> HandleAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (GlobalAppException ex)
        {
            _logger.LogWarning(ex, "Business exception: {ErrorCode}", ex.ErrorCode);

            return BadRequest(new
            {
                statusCode = 400,
                error = ex.ErrorCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error");

            return StatusCode(500, new
            {
                statusCode = 500,
                error = "SERVER_ERROR"
            });
        }
    }

    private IActionResult InvalidInputResponse()
    {
        return BadRequest(new
        {
            statusCode = 400,
            error = "INVALID_INPUT"
        });
    }
}