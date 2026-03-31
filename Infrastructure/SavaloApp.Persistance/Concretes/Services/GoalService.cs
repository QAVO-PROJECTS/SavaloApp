using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
using SavaloApp.Application.Abstracts.Repositories.Goals;
using SavaloApp.Application.Abstracts.Repositories.GoalSections;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.Goal;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

public class GoalService : IGoalService
{
    private readonly IGoalReadRepository _readRepo;
    private readonly IGoalWriteRepository _writeRepo;
    private readonly IMapper _mapper;
    private readonly IGoalSectionReadRepository _goalSectionReadRepository;
    private readonly ICurrencyAccountReadRepository _currencyReadRepository;

    public GoalService(
        IGoalReadRepository readRepo,
        IGoalWriteRepository writeRepo,
        IMapper mapper,
        IGoalSectionReadRepository goalSectionReadRepository,
        ICurrencyAccountReadRepository currencyReadRepository)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _mapper = mapper;
        _goalSectionReadRepository = goalSectionReadRepository;
        _currencyReadRepository = currencyReadRepository;
    }

    public async Task<List<GoalDto>> GetAllAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,
            include: q => q
                .Include(x => x.GoalSection)
                .Include(x => x.CurrencyAccount)
                .Include(x => x.GoalTransactions));

        return _mapper.Map<List<GoalDto>>(entities);
    }

    public async Task<GoalDto> GetByIdAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,
            include: q => q
                .Include(x => x.GoalSection)
                .Include(x => x.CurrencyAccount)
                .Include(x => x.GoalTransactions))
            ?? throw new GlobalAppException("GOAL_NOT_FOUND");

        return _mapper.Map<GoalDto>(entity);
    }

    public async Task<GoalDto> CreateAsync(CreateGoalDto dto, string userId)
    {
        Guid currencyId;

        // 🔥 currency fix
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

        // 🔥 section default logic
        if (!string.IsNullOrWhiteSpace(dto.GoalSectionId))
        {
            var sectionId = ParseGuid(dto.GoalSectionId);

            var section = await _goalSectionReadRepository
                .GetAsync(x => x.Id == sectionId);

            if (section == null)
                throw new GlobalAppException("GOAL_SECTION_NOT_FOUND");

            dto.Name ??= section.Name;
            dto.ColorCode ??= "#FFFFFF";
        }

        var entity = _mapper.Map<Goal>(dto);

        entity.Id = Guid.NewGuid();
        entity.CurrencyAccountId = currencyId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<GoalDto>(entity);
    }

    public async Task UpdateAsync(UpdateGoalDto dto, string userId)
    {
        var gid = ParseGuid(dto.Id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId)
            ?? throw new GlobalAppException("GOAL_NOT_FOUND");

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
            ?? throw new GlobalAppException("GOAL_NOT_FOUND");

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