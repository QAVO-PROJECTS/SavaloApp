using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.CategoryTransactions;
using SavaloApp.Application.Abstracts.Repositories.Categories;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CategoryTransaction;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class CategoryTransactionService : ICategoryTransactionService
{
    private readonly ICategoryTransactionReadRepository _readRepo;
    private readonly ICategoryTransactionWriteRepository _writeRepo;
    private readonly ICategoryReadRepository _categoryReadRepo;
    private readonly ICategoryWriteRepository _categoryWriteRepo;
    
    private readonly IMapper _mapper;

    public CategoryTransactionService(
        ICategoryTransactionReadRepository readRepo,
        ICategoryTransactionWriteRepository writeRepo,
        ICategoryReadRepository categoryReadRepo,
        ICategoryWriteRepository categoryWriteRepo,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _categoryReadRepo = categoryReadRepo;
        _categoryWriteRepo = categoryWriteRepo;
        _mapper = mapper;
    }

    public async Task<List<CategoryTransactionDto>> GetAllAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.Category.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Category).ThenInclude(x => x.CategorySection));

        return _mapper.Map<List<CategoryTransactionDto>>(entities);
    }
    public async Task<List<CategoryTransactionDto>> GetAllForCategoryAsync(string categoryId,string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
                !x.IsDeleted && x.CategoryId.ToString()==categoryId &&
                x.Category.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Category).ThenInclude(x => x.CategorySection));

        return _mapper.Map<List<CategoryTransactionDto>>(entities);
    }

    public async Task<CategoryTransactionDto> GetByIdAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.Category.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Category).ThenInclude(x => x.CategorySection))
            ?? throw new GlobalAppException("CATEGORY_TRANSACTION_NOT_FOUND");

        return _mapper.Map<CategoryTransactionDto>(entity);
    }

    public async Task<CategoryTransactionDto> CreateAsync(CreateCategoryTransactionDto dto, string userId)
    {
        var categoryId = ParseGuid(dto.CategoryId);

        var category = await _categoryReadRepo.GetAsync(x =>
                           x.Id == categoryId &&
                           !x.IsDeleted &&
                           x.CurrencyAccount.UserId == userId)
                       ?? throw new GlobalAppException("CATEGORY_NOT_FOUND");

        // ❗ Cash out üçün yoxlama
        if (dto.TransactionType) // expense
        {
            if (dto.Amount > category.Amount)
                throw new GlobalAppException("INSUFFICIENT_CATEGORY_BALANCE");

            category.Amount -= dto.Amount; // 🔥 AZALT
        }
        else // income
        {
            category.Amount += dto.Amount; // 🔥 ARTIR
        }

        var entity = _mapper.Map<CategoryTransaction>(dto);

        entity.Id = Guid.NewGuid();
        entity.CategoryId = categoryId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        await _writeRepo.AddAsync(entity);

        // 🔥 CATEGORY UPDATE ETMƏYİ UNUTMA
        await _categoryWriteRepo.UpdateAsync(category);

        await _writeRepo.CommitAsync();

        return _mapper.Map<CategoryTransactionDto>(entity);
    }



    public async Task DeleteAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.Category.CurrencyAccount.UserId == userId)
            ?? throw new GlobalAppException("CATEGORY_TRANSACTION_NOT_FOUND");

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