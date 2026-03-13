using SavaloApp.Application.Abstracts.Repositories.TermsAndConditions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.TermsAndConditions;

public class TermsAndConditionWriteRepository:WriteRepository<TermsAndCondition>,ITermsAndConditionWriteRepository
{
    public TermsAndConditionWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}