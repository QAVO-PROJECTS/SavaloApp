using SavaloApp.Application.Abstracts.Repositories.Categories;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Categories;

public class CategoryReadRepository:ReadRepository<Category>,ICategoryReadRepository
{
    public CategoryReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}