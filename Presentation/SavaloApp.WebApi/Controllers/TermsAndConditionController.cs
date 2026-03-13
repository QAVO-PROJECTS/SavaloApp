using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.TermsAndCondition;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/terms-and-conditions")]
public class TermsAndConditionController : ControllerBase
{
    private readonly ITermsAndConditionService _termsAndConditionService;
    private readonly ILogger<TermsAndConditionController> _logger;

    public TermsAndConditionController(
        ITermsAndConditionService termsAndConditionService,
        ILogger<TermsAndConditionController> logger)
    {
        _termsAndConditionService = termsAndConditionService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetById()
    {
 

        return await HandleAsync(async () =>
        {
            var result = await _termsAndConditionService.GetAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "TERMS_AND_CONDITION_GET_SUCCESS",
                data = result
            });
        });
    }



    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTermsAndConditionDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
           await _termsAndConditionService.CreateAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "TERMS_AND_CONDITION_CREATE_SUCCESS",
                data = ""
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update( [FromBody] UpdateTermsAndConditionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id) || !ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
             await _termsAndConditionService.UpdateAsync( dto);

            return Ok(new
            {
                statusCode = 200,
                message = "TERMS_AND_CONDITION_UPDATE_SUCCESS",
                data = ""
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
            await _termsAndConditionService.DeleteAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "TERMS_AND_CONDITION_DELETE_SUCCESS"
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