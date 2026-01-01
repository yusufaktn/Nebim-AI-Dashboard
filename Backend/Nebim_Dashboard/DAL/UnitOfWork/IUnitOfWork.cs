using DAL.Repositories;
using Entity.Base;

namespace DAL.UnitOfWork;

/// <summary>
/// Unit of Work interface - Transaction yönetimi ve repository erişimi
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Generic Repository - Herhangi bir Entity için kullanılabilir
    /// Örnek: unitOfWork.Repository<User>().GetByIdAsync(1)
    /// </summary>
    IGenericRepository<T> Repository<T>() where T : class, IEntity<int>;
    
    /// <summary>
    /// Nebim Repository (Read-only)
    /// </summary>
    INebimRepository Nebim { get; }
    
    /// <summary>
    /// Değişiklikleri veritabanına kaydet
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Transaction başlat
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Transaction'ı onayla
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Transaction'ı geri al
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Audit kullanıcısını ayarla
    /// </summary>
    void SetAuditUser(string? userId);
}
