using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.Categories;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.Category;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryReadRepository _readRepo;
    private readonly ICategoryWriteRepository _writeRepo;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryReadRepository readRepo,
        ICategoryWriteRepository writeRepo,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId);

        return _mapper.Map<List<CategoryDto>>(entities);
    }

    public async Task<CategoryDto> GetByIdAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.CurrencyAccount))
            ?? throw new GlobalAppException("CATEGORY_NOT_FOUND");

        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, string userId)
    {
        var currencyId = ParseGuid(dto.CurrencyAccountId);

        var entity = _mapper.Map<Category>(dto);

        entity.Id = Guid.NewGuid();
        entity.CurrencyAccountId = currencyId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task UpdateAsync(UpdateCategoryDto dto, string userId)
    {
        var gid = ParseGuid(dto.Id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId)
            ?? throw new GlobalAppException("CATEGORY_NOT_FOUND");

        _mapper.Map(dto, entity);

        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId)
            ?? throw new GlobalAppException("CATEGORY_NOT_FOUND");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _writeRepo.UpdateAsync(entity);
        await _writeRepo.CommitAsync();
    }

    private static Guid ParseGuid(string id)
    {
        if (!Guid.TryParse(id, out var gid))
            throw new GlobalAppException("INVALID_ID");

        return gid;
    }
}