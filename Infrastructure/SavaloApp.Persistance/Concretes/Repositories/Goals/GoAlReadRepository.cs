using SavaloApp.Application.Abstracts.Repositories.Goals;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Goals;

public class GoAlReadRepository:ReadRepository<Goal>,IGoalReadRepository
{
    public GoAlReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}