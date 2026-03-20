using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyArtPlace.Core.Interfaces;

namespace MyArtPlace.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContextFactory _factory;

    public Repository(DbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<T>> GetAllAsync()
    {
        using var db = _factory.CreateDbContext();
        return await db.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        using var db = _factory.CreateDbContext();
        return await db.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        using var db = _factory.CreateDbContext();
        return await db.Set<T>().AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        using var db = _factory.CreateDbContext();
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        using var db = _factory.CreateDbContext();
        db.Set<T>().Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = _factory.CreateDbContext();
        var entity = await db.Set<T>().FindAsync(id);
        if (entity is not null)
        {
            db.Set<T>().Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
