using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

public class GenericRepository<TEntity>(DbContext dbContext) : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _dbContext = dbContext;

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, Expression<Func<TEntity, object>>[]? includes = null, CancellationToken cancellationToken = default)
    {
        if (includes == null || includes.Length == 0)
        {
            return await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        }

        var primaryKeyName = _dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties
            .Select(x => x.Name).SingleOrDefault();

        if (string.IsNullOrEmpty(primaryKeyName))
            throw new InvalidOperationException($"No se encontró la clave primaria para {typeof(TEntity).Name}");

        var query = ApplyIncludes(includes);

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, primaryKeyName) == id, cancellationToken);
    }
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(Expression<Func<TEntity, object>>[]? includes = null, CancellationToken cancellationToken = default)
    {
        var query = ApplyIncludes(includes);
        return await query.ToListAsync(cancellationToken);
    }
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>>[]? includes = null, CancellationToken cancellationToken = default)
    {
        var query = ApplyIncludes(includes).Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var primaryKeyName = _dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties
            .Select(x => x.Name).SingleOrDefault();

        if (string.IsNullOrEmpty(primaryKeyName)) return false;

        return await _dbContext.Set<TEntity>().AnyAsync(e => EF.Property<Guid>(e, primaryKeyName) == id, cancellationToken);
    }
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        return entity;
    }
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }
    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity != null)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }
    }
    private IQueryable<TEntity> ApplyIncludes(Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> query = _dbContext.Set<TEntity>();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return query;
    }
}