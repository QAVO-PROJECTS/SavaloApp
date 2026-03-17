using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly ILogger<UserAuthController> _logger;

    public UserAuthController(
        IUserAuthService userAuthService,
        ILogger<UserAuthController> logger)
    {
        _userAuthService = userAuthService;
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            throw new GlobalAppException("USER_NOT_FOUND");

        return userId;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.RegisterAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "REGISTER_SUCCESS",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.LoginAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "LOGIN_SUCCESS",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.UserId))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.SendOtpAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "OTP_SENT",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("verify-user")]
    public async Task<IActionResult> VerifyUser([FromBody] VerifyOtpRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null ||
            string.IsNullOrWhiteSpace(dto.UserId) ||
            string.IsNullOrWhiteSpace(dto.Code))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.VerifyUserAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "USER_VERIFIED",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.IdToken))
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = "INVALID_GOOGLE_TOKEN"
            });
        }

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.GoogleLoginAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "GOOGLE_LOGIN_SUCCESS",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("apple-login")]
    public async Task<IActionResult> AppleLogin([FromBody] AppleLoginRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.IdentityToken))
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = "INVALID_APPLE_TOKEN"
            });
        }

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.AppleLoginAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "APPLE_LOGIN_SUCCESS",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("social/complete-phone")]
    public async Task<IActionResult> CompleteSocialPhone([FromBody] CompleteSocialPhoneRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.CompleteSocialPhoneAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "PHONE_VERIFICATION_CODE_SENT",
                data = result
            });
        });
    }

    [AllowAnonymous]
    [HttpPost("social/verify-phone")]
    public async Task<IActionResult> VerifySocialPhone([FromBody] VerifySocialPhoneOtpRequestDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _userAuthService.VerifySocialPhoneOtpAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "PHONE_VERIFIED_SUCCESS",
                data = result
            });
        });
    }

    [Authorize]
    [HttpPost("start-email-update")]
    public async Task<IActionResult> StartEmailUpdate([FromBody] StartEmailUpdateDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Email))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _userAuthService.StartEmailUpdateAsync(userId, dto);

            return Ok(new
            {
                statusCode = 200,
                message = "EMAIL_OTP_SENT",
                data = result
            });
        });
    }

    [Authorize]
    [HttpPost("verify-email-otp")]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Code))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _userAuthService.VerifyEmailOtpAsync(userId, dto);

            return Ok(new
            {
                statusCode = 200,
                message = "EMAIL_VERIFIED_SUCCESS",
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
            var result = await _userAuthService.RefreshTokenAsync(dto.RefreshToken);

            return Ok(new
            {
                statusCode = 200,
                message = "TOKEN_REFRESH_SUCCESS",
                data = result
            });
        });
    }

    [Authorize]
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
            await _userAuthService.RevokeRefreshTokenAsync(dto.RefreshToken);

            return Ok(new
            {
                statusCode = 200,
                message = "TOKEN_REVOKED_SUCCESS"
            });
        });
    }
}