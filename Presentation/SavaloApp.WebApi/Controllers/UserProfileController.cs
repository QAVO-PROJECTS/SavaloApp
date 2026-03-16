using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.UserAuth;

using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileService userProfileService,
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
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

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new GlobalAppException("UNAUTHORIZED");

        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _userProfileService.GetProfileAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "PROFILE_GET_SUCCESS",
                data = result
            });
        });
    }

    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        if (dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _userProfileService.UpdateProfileAsync(userId, dto);

            return Ok(new
            {
                statusCode = 200,
                message = "PROFILE_UPDATE_SUCCESS",
                data = result
            });
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            await _userProfileService.ChangePasswordAsync(userId, dto);

            return Ok(new
            {
                statusCode = 200,
                message = "PASSWORD_CHANGE_SUCCESS"
            });
        });
    }
}