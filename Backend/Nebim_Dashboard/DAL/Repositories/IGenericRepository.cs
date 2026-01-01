using System.Linq.Expressions;
using Entity.Base;
using Entity.DTOs.Common;

namespace DAL.Repositories;

/// <summary>
/// Generic Repository Interface - Tek interface, tüm CRUD işlemleri
/// </summary>
public interface IGenericRepository<T> where T : class, IEntity<int>
{
    // Read
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    
    // Paged
    Task<PagedResult<T>> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        CancellationToken ct = default);
    
    // Write
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    
    // Query (advanced)
    IQueryable<T> Query();
}
