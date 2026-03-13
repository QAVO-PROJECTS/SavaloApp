using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.OtpCodes;

public class OtpCodeReadRepository:ReadRepository<OtpCode>,IOtpCodeReadRepository
{
    public OtpCodeReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}