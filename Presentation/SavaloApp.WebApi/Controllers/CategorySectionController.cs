using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CategorySection;
using SavaloApp.Application.GlobalException;

namespace SavaloApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/category-sections")]
public class CategorySectionController : ControllerBase
{
    private readonly ICategorySectionService _categorySectionService;
    private readonly ILogger<CategorySectionController> _logger;

    public CategorySectionController(
        ICategorySectionService categorySectionService,
        ILogger<CategorySectionController> logger)
    {
        _categorySectionService = categorySectionService;
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
            var result = await _categorySectionService.GetCategorySectionAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "CATEGORY_SECTION_GET_SUCCESS",
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
            var result = await _categorySectionService.GetAllCategorySectionsAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "CATEGORY_SECTION_LIST_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCategorySectionDto dto)
    {
        if (!ModelState.IsValid || dto == null)
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _categorySectionService.CreateCategorySectionAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "CATEGORY_SECTION_CREATE_SUCCESS",
                data = result
            });
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateCategorySectionDto dto)
    {
        if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Id))
            return InvalidInputResponse();

        return await HandleAsync(async () =>
        {
            var result = await _categorySectionService.UpdateCategorySectionAsync(dto);

            return Ok(new
            {
                statusCode = 200,
                message = "CATEGORY_SECTION_UPDATE_SUCCESS",
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
            await _categorySectionService.DeleteCategorySectionAsync(id);

            return Ok(new
            {
                statusCode = 200,
                message = "CATEGORY_SECTION_DELETE_SUCCESS"
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