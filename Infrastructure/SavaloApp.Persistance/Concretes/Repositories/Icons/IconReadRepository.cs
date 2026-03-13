using SavaloApp.Application.Abstracts.Repositories.Icons;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Icons;

public class IconReadRepository:ReadRepository<Icon>,IIconReadRepository
{
    public IconReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}