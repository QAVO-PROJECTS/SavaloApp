using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.Categories;
using SavaloApp.Application.Abstracts.Repositories.CategorySections;
using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
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
    private readonly ICategorySectionReadRepository _categorySectionReadRepository;
    private readonly ICurrencyAccountReadRepository _currencyReadRepository;

    public CategoryService(
        ICategoryReadRepository readRepo,
        ICategoryWriteRepository writeRepo,
        IMapper mapper,
         ICategorySectionReadRepository categorySectionReadRepository,
         ICurrencyAccountReadRepository currencyReadRepository)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _mapper = mapper;
        _categorySectionReadRepository = categorySectionReadRepository;
        _currencyReadRepository = currencyReadRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,include:q=>q.Include(x=>x.CategorySection).Include(x=>x.CurrencyAccount).Include(x=>x.CategoryTransactions));

        return _mapper.Map<List<CategoryDto>>(entities);
    }

    public async Task<CategoryDto> GetByIdAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,
            include: q => q.Include(x=>x.CategorySection).Include(x=>x.CurrencyAccount).Include(x=>x.CategoryTransactions))
            ?? throw new GlobalAppException("CATEGORY_NOT_FOUND");

        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, string userId)
    {
      Guid currencyId;
        if (string.IsNullOrWhiteSpace(dto.CurrencyAccountId))
        {
            var currency = await _currencyReadRepository
                .GetAsync(x => x.UserId == userId && !x.IsDeleted);

            if (currency == null)
                throw new GlobalAppException("CURRENCY_NOT_FOUND");

            currencyId = currency.Id;
        }
        else
        {
            currencyId = ParseGuid(dto.CurrencyAccountId);
        }

        // 🔥 ƏVVƏL DTO düzəlt
        if (!string.IsNullOrWhiteSpace(dto.CategorySectionId))
        {
            var sectionId = ParseGuid(dto.CategorySectionId);

            var section = await _categorySectionReadRepository
                .GetAsync(x => x.Id == sectionId);

            if (section == null)
                throw new GlobalAppException("CATEGORY_SECTION_NOT_FOUND");

            // ✅ default doldur
            dto.Name ??= section.Name;
            dto.Amount = 0;
            dto.StartDate = DateTime.UtcNow.AddHours(4).ToString("dd.MM.yyyy");

            // 👉 repeat null-dursa default
            dto.RepeatType ??= RepeatType.None;

            dto.ColorCode ??= "#FFFFFF";
        }

        // 🔥 SONRA MAP ET
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