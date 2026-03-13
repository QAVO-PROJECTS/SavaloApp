using SavaloApp.Application.Abstracts.Repositories;
using SavaloApp.Application.Abstracts.Repositories.RefreshTokens;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.RefreshTokens;

public class RefreshTokenWriteRepository:WriteRepository<RefreshToken>,IRefreshTokenWriteRepository
{
    public RefreshTokenWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}