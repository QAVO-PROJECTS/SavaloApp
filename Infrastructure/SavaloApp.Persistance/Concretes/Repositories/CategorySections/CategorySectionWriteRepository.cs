using SavaloApp.Application.Abstracts.Repositories.CategorySections;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CategorySections;

public class CategorySectionWriteRepository:WriteRepository<CategorySection>,ICategorySectionWriteRepository
{
    public CategorySectionWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}