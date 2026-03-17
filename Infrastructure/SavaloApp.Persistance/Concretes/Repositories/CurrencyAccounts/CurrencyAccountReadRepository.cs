using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CurrencyAccounts;

public class CurrencyAccountReadRepository:ReadRepository<CurrencyAccount>,ICurrencyAccountReadRepository
{
    public CurrencyAccountReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}