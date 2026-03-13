using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.AdminAuth;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(IAdminAuthService adminAuthService, ILogger<AdminAuthController> logger)
    {
        _adminAuthService = adminAuthService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _adminAuthService.LoginAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "LOGIN_SUCCESS",
                data = result
            });
        });
    }
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = "REFRESH_TOKEN_REQUIRED"
            });
        }

        return await HandleAsync(async () =>
        {
            var result = await _adminAuthService.RefreshTokenAsync(dto.RefreshToken);

            return Ok(new
            {
                statusCode = 200,
                message = "TOKEN_REFRESH_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = "REFRESH_TOKEN_REQUIRED"
            });
        }

        return await HandleAsync(async () =>
        {
            await _adminAuthService.RevokeRefreshTokenAsync(dto.RefreshToken);

            return Ok(new
            {
                statusCode = 200,
                message = "TOKEN_REVOKED_SUCCESS"
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