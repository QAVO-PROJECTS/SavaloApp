using SavaloApp.Application.Abstracts.Repositories.CategorySections;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CategorySections;

public class CategorySectionReadRepository:ReadRepository<CategorySection>,ICategorySectionReadRepository
    
{
    public CategorySectionReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}