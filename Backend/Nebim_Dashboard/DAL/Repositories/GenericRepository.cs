using System.Linq.Expressions;
using DAL.Context;
using Entity.Base;
using Entity.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

/// <summary>
/// Generic Repository - Tüm entity'ler için tek implementation
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity<int>
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Read

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(predicate).ToListAsync(ct);

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, ct);

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate == null ? await _dbSet.CountAsync(ct) : await _dbSet.CountAsync(predicate, ct);

    #endregion

    #region Paged

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(ct);

        if (orderBy != null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        else
            query = query.OrderBy(e => e.Id);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<T>.Create(items, page, pageSize, totalCount);
    }

    #endregion

    #region Write

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        var entry = await _dbSet.AddAsync(entity, ct);
        return entry.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(entities, ct);

    public virtual void Update(T entity)
        => _dbSet.Update(entity);

    public virtual void Delete(T entity)
        => _dbSet.Remove(entity);

    #endregion

    #region Query

    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

    #endregion
}
