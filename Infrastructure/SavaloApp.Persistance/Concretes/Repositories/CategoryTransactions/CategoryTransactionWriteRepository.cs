using SavaloApp.Application.Abstracts.Repositories;
using SavaloApp.Application.Abstracts.Repositories.CategoryTransactions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CategoryTransactions;

public class CategoryTransactionWriteRepository:WriteRepository<CategoryTransaction>,ICategoryTransactionWriteRepository
{
    public CategoryTransactionWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}