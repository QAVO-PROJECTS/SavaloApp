using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.Icon;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/icons")]
public class IconController : ControllerBase
{
    private readonly IIconService _IconService;
    private readonly ILogger<IconController> _logger;

    public IconController(
        IIconService IconService,
        ILogger<IconController> logger)
    {
        _IconService = IconService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _IconService.GetIconAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "ICON_GET_SUCCESS",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await HandleAsync(async () =>
        {
            var result = await _IconService.GetAllIconsAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "ICON_LIST_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateIconDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _IconService.CreateIconAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "ICON_CREATE_SUCCESS",
                data = result
            });
        });
    }



    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            await _IconService.DeleteIconAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "ICON_DELETE_SUCCESS"
            });
        });
    }

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