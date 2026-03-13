using SavaloApp.Application.Abstracts.Repositories.GoalSections;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.GoalSections;

public class GoalSectionWriteRepository:WriteRepository<GoalSection>,IGoalSectionWriteRepository
{
    public GoalSectionWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}