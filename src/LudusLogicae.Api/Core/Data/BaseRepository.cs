using LudusLogicae.Api.Core.Common;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Core.Data;

public abstract class BaseRepository<TEntity>(AppDbContext dbContext)
    where TEntity : class, IEntity
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken,
        bool asNoTracking = true)
    {
        var query = _dbSet.AsQueryable();
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
