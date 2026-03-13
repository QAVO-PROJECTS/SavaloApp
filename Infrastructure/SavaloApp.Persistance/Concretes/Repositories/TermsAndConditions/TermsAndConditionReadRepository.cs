using SavaloApp.Application.Abstracts.Repositories.TermsAndConditions;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.TermsAndConditions;

public class TermsAndConditionReadRepository:ReadRepository<TermsAndCondition>,ITermsAndConditionReadRepository
{
    public TermsAndConditionReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}