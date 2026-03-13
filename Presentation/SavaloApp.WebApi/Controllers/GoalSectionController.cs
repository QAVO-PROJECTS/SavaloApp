using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.GoalSection;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/goal-sections")]
public class GoalSectionController : ControllerBase
{
    private readonly IGoalSectionService _GoalSectionService;
    private readonly ILogger<GoalSectionController> _logger;

    public GoalSectionController(
        IGoalSectionService GoalSectionService,
        ILogger<GoalSectionController> logger)
    {
        _GoalSectionService = GoalSectionService;
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
            var result = await _GoalSectionService.GetGoalSectionAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_SECTION_GET_SUCCESS",
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
            var result = await _GoalSectionService.GetAllGoalSectionsAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_SECTION_LIST_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateGoalSectionDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _GoalSectionService.CreateGoalSectionAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_SECTION_CREATE_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateGoalSectionDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _GoalSectionService.UpdateGoalSectionAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_SECTION_UPDATE_SUCCESS",
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
            await _GoalSectionService.DeleteGoalSectionAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAl_SECTION_DELETE_SUCCESS"
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