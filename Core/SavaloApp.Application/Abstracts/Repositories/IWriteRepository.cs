using Microsoft.EntityFrameworkCore;

namespace SavaloApp.Application.Abstracts.Repositories;

public interface IWriteRepository<T> where T : class, new()
{
    Task AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task HardDeleteAsync(T entity);
    Task<int> CommitAsync();

    Task SoftDeleteAsync(T entity);
    DbContext GetDbContext();
}