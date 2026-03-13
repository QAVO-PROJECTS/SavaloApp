using SavaloApp.Application.Abstracts.Repositories.RefreshTokens;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.RefreshTokens;

public class RefreshTokenReadRepository:ReadRepository<RefreshToken>,IRefreshTokenReadRepository
    
{
    public RefreshTokenReadRepository(SavaloAppDbContext dbContext) : base(dbContext)
    {
    }
}