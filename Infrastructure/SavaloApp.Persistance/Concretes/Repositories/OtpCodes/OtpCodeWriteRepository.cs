using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.OtpCodes;

public class OtpCodeWriteRepository:WriteRepository<OtpCode>,IOtpCodeWriteRepository
{
    public OtpCodeWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}