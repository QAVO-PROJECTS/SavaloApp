using SavaloApp.Application.Abstracts.Repositories.GoalTransactions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.GoalTransactions;

public class GoalTransactionWriteRepository:WriteRepository<GoalTransaction>,IGoalTransactionWriteRepository
{
    public GoalTransactionWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}