using Entity.App;
using Entity.Base;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

/// <summary>
/// PostgreSQL AppDB için DbContext
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    
    // Multi-tenant DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<QueryQuota> QueryQuotas => Set<QueryQuota>();
    public DbSet<QueryHistory> QueryHistories => Set<QueryHistory>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tüm configuration'ları otomatik yükle
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        // PostgreSQL için timestamp with time zone kullan
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
    
    /// <summary>
    /// Audit alanlarını otomatik güncelle
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        var now = DateTime.UtcNow;
        
        foreach (var entry in entries)
        {
            // BaseEntity için CreatedAt/UpdatedAt
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = now;
                }
                else
                {
                    baseEntity.UpdatedAt = now;
                }
            }
            
            // ISoftDeletable için DeletedAt
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Modified)
            {
                var isDeletedProperty = entry.Property(nameof(ISoftDeletable.IsDeleted));
                if (isDeletedProperty.IsModified && softDeletable.IsDeleted)
                {
                    softDeletable.DeletedAt = now;
                }
            }
        }
    }
    
    /// <summary>
    /// Audit alanlarını kullanıcı bilgisiyle güncelle
    /// </summary>
    public void SetAuditUser(string? userId)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));
        
        foreach (var entry in entries)
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedBy = userId;
            }
            else
            {
                auditableEntity.UpdatedBy = userId;
            }
        }
    }
    
    /// <summary>
    /// Soft delete için audit user set et
    /// </summary>
    public void SetDeletedBy(string? userId)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            var softDeletable = (ISoftDeletable)entry.Entity;
            var isDeletedProperty = entry.Property(nameof(ISoftDeletable.IsDeleted));
            
            if (isDeletedProperty.IsModified && softDeletable.IsDeleted)
            {
                softDeletable.DeletedBy = userId;
            }
        }
    }
}
