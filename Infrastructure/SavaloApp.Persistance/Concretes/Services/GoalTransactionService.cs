using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.Goals;
using SavaloApp.Application.Abstracts.Repositories.GoalTransactions;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.GoalTransaction;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class GoalTransactionService : IGoalTransactionService
{
    private readonly IGoalTransactionReadRepository _readRepo;
    private readonly IGoalTransactionWriteRepository _writeRepo;
    private readonly IGoalReadRepository _goalReadRepo;
    private readonly IMapper _mapper;

    public GoalTransactionService(
        IGoalTransactionReadRepository readRepo,
        IGoalTransactionWriteRepository writeRepo,
        IGoalReadRepository goalReadRepo,
        IMapper mapper)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
        _goalReadRepo = goalReadRepo;
        _mapper = mapper;
    }

    public async Task<List<GoalTransactionDto>> GetAllAsync(string userId)
    {
        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.Goal.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Goal));

        return _mapper.Map<List<GoalTransactionDto>>(entities);
    }

    public async Task<List<GoalTransactionDto>> GetAllForGoalAsync(string goalId, string userId)
    {
        var gid = ParseGuid(goalId);

        var entities = await _readRepo.GetAllAsync(x =>
            !x.IsDeleted &&
            x.GoalId == gid &&
            x.Goal.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Goal));

        return _mapper.Map<List<GoalTransactionDto>>(entities);
    }

    public async Task<GoalTransactionDto> GetByIdAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.Goal.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.Goal))
            ?? throw new GlobalAppException("GOAL_TRANSACTION_NOT_FOUND");

        return _mapper.Map<GoalTransactionDto>(entity);
    }

    public async Task<GoalTransactionDto> CreateAsync(CreateGoalTransactionDto dto, string userId)
    {
        var goalId = ParseGuid(dto.GoalId);

        var goal = await _goalReadRepo.GetAsync(x =>
            x.Id == goalId &&
            !x.IsDeleted &&
            x.CurrencyAccount.UserId == userId,
            include: q => q.Include(x => x.GoalTransactions))
            ?? throw new GlobalAppException("GOAL_NOT_FOUND");

 
        var currentBalance = goal.GoalTransactions != null
            ? goal.GoalTransactions.Sum(x => x.TransactionType ? -x.Amount : x.Amount)
            : 0;

        if (dto.TransactionType) // CASH OUT
        {
            if (dto.Amount > currentBalance)
                throw new GlobalAppException("INSUFFICIENT_GOAL_BALANCE");
        }

        var entity = _mapper.Map<GoalTransaction>(dto);

        entity.Id = Guid.NewGuid();
        entity.GoalId = goalId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        await _writeRepo.AddAsync(entity);
        await _writeRepo.CommitAsync();

        return _mapper.Map<GoalTransactionDto>(entity);
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var gid = ParseGuid(id);

        var entity = await _readRepo.GetAsync(x =>
            x.Id == gid &&
            !x.IsDeleted &&
            x.Goal.CurrencyAccount.UserId == userId)
            ?? throw new GlobalAppException("GOAL_TRANSACTION_NOT_FOUND");

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