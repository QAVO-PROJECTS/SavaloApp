using SavaloApp.Application.Abstracts.Repositories.GoalTransactions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.GoalTransactions;

public class GoalTransactionReadRepository:ReadRepository<GoalTransaction>,IGoalTransactionReadRepository
{
    public GoalTransactionReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}