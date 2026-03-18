using SavaloApp.Application.Abstracts.Repositories.Categories;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Categories;

public class CategoryWriteRepository:WriteRepository<Category>,ICategoryWriteRepository
    
{
    public CategoryWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}