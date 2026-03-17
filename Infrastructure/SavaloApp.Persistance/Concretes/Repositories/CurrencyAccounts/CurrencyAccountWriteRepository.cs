using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.CurrencyAccounts;

public class CurrencyAccountWriteRepository:WriteRepository<CurrencyAccount>,ICurrencyAccountWriteRepository
{
    public CurrencyAccountWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}