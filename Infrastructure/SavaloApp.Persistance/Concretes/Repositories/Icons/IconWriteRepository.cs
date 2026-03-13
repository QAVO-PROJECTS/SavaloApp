using SavaloApp.Application.Abstracts.Repositories.Icons;
using SavaloApp.Domain.Entities;
using SavaloApp.Persistance.Context;

namespace SavaloApp.Persistance.Concretes.Repositories.Icons;

public class IconWriteRepository:WriteRepository<Icon>,IIconWriteRepository
{
    public IconWriteRepository(SavaloAppDbContext SavaloAppDbContext) : base(SavaloAppDbContext)
    {
    }
}