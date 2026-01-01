using DAL.Context;
using DAL.Repositories;
using Entity.Base;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.UnitOfWork;

/// <summary>
/// Unit of Work implementation - Generic Repository pattern ile
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly INebimRepository _nebimRepository;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed;
    
    public UnitOfWork(AppDbContext context, INebimRepository nebimRepository)
    {
        _context = context;
        _nebimRepository = nebimRepository;
    }
    
    #region Repositories
    
    /// <summary>
    /// Generic Repository - İlk çağrıda oluşturulur ve cache'lenir
    /// </summary>
    public IGenericRepository<T> Repository<T>() where T : class, IEntity<int>
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            var repository = new GenericRepository<T>(_context);
            _repositories.Add(type, repository);
        }
        
        return (IGenericRepository<T>)_repositories[type];
    }
    
    public INebimRepository Nebim => _nebimRepository;
    
    #endregion
    
    #region Save Changes
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
    
    #endregion
    
    #region Transaction Management
    
    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);
    
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    #endregion
    
    #region Audit
    
    public void SetAuditUser(string? userId)
        => _context.SetAuditUser(userId);
    
    #endregion
    
    #region Dispose
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
    
    #endregion
}
