using SavaloApp.Application.Abstracts.Repositories.GoalSections;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.GoalSections;

public class GoalSectionReadRepository:ReadRepository<GoalSection>,IGoalSectionReadRepository
{
    public GoalSectionReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}