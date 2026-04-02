using SavaloApp.Application.Abstracts.Repositories;
using SavaloApp.Application.Abstracts.Repositories.CategoryTransactions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CategoryTransactions;

public  class CategoryTransactionReadRepository:ReadRepository<CategoryTransaction>,ICategoryTransactionReadRepository
{
    public CategoryTransactionReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}