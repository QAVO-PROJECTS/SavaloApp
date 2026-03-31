using SavaloApp.Application.Abstracts.Repositories.Goals;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Goals;

public class GoalWriteRepository:WriteRepository<Goal>,IGoalWriteRepository
{
    public GoalWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}