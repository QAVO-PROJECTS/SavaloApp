using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.GoalTransaction;
using SavaloApp.Application.GlobalException;

[ApiController]
[Route("api/goal-transactions")]
[Authorize(Roles = "Customer")]
public class GoalTransactionsController : ControllerBase
{
    private readonly IGoalTransactionService _service;
    private readonly ILogger<GoalTransactionsController> _logger;

    public GoalTransactionsController(
        IGoalTransactionService service,
        ILogger<GoalTransactionsController> logger)
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
            var result = await _service.GetAllAsync(userId);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_TRANSACTION_LIST_SUCCESS",
                data = result
            });
        });
    }

    [HttpGet("goal/{goalId}")]
    public async Task<IActionResult> GetAllForGoal(string goalId)
    {
        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _service.GetAllForGoalAsync(goalId, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_TRANSACTION_LIST_SUCCESS",
                data = result
            });
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInput();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _service.GetByIdAsync(id, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_TRANSACTION_GET_SUCCESS",
                data = result
            });
        });
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateGoalTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return InvalidInput();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            var result = await _service.CreateAsync(dto, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_TRANSACTION_CREATE_SUCCESS",
                data = result
            });
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return InvalidInput();

        return await HandleAsync(async () =>
        {
            var userId = GetUserId();
            await _service.DeleteAsync(id, userId);

            return Ok(new
            {
                statusCode = 200,
                message = "GOAL_TRANSACTION_DELETE_SUCCESS"
            });
        });
    }

    private string GetUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    private async Task<IActionResult> HandleAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (GlobalAppException ex)
        {
            _logger.LogWarning(ex, ex.ErrorCode);
            return BadRequest(new { statusCode = 400, error = ex.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error");
            return StatusCode(500, new { statusCode = 500, error = "SERVER_ERROR" });
        }
    }

    private IActionResult InvalidInput()
        => BadRequest(new { statusCode = 400, error = "INVALID_INPUT" });
}