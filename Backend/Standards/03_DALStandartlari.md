# 🗄️ DAL (Data Access Layer) Standartları

> Bu doküman, Data Access Layer katmanındaki Repository pattern, Dapper ve EF Core kullanımı, N+1 query problemi ve çözümlerini tanımlar.

---

## 📌 İçindekiler

1. [Klasör Yapısı](#1-klasör-yapısı)
2. [Dual Database Stratejisi](#2-dual-database-stratejisi)
3. [Repository Pattern](#3-repository-pattern)
4. [Nebim Repository (Dapper)](#4-nebim-repository-dapper)
5. [App Repository (EF Core)](#5-app-repository-ef-core)
6. [N+1 Query Problemi ve Çözümleri](#6-n1-query-problemi-ve-çözümleri)
7. [Unit of Work Pattern](#7-unit-of-work-pattern)
8. [Connection Yönetimi](#8-connection-yönetimi)

---

## 1. Klasör Yapısı

```
DAL/
├── Context/
│   └── AppDbContext.cs              # EF Core DbContext (AppDB için)
│
├── Configurations/                   # EF Core entity konfigürasyonları
│   ├── UserConfiguration.cs
│   ├── ChatSessionConfiguration.cs
│   └── ChatMessageConfiguration.cs
│
├── Repositories/
│   ├── Interfaces/                   # Repository interface'leri
│   │   ├── INebimRepository.cs       # Nebim V3 okuma işlemleri
│   │   ├── IUserRepository.cs
│   │   ├── IChatRepository.cs
│   │   └── ITargetRepository.cs
│   │
│   ├── Nebim/                        # Nebim V3 repository'leri (Dapper)
│   │   ├── NebimRepository.cs        # Gerçek implementasyon
│   │   └── MockNebimRepository.cs    # Mock implementasyon (geliştirme için)
│   │
│   └── App/                          # AppDB repository'leri (EF Core)
│       ├── UserRepository.cs
│       ├── ChatRepository.cs
│       └── TargetRepository.cs
│
├── Extensions/
│   └── DapperExtensions.cs           # Dapper için yardımcı metodlar
│
└── UnitOfWork/
    ├── IUnitOfWork.cs
    └── UnitOfWork.cs
```

---

## 2. Dual Database Stratejisi

### 2.1 Veritabanı Ayrımı

| Veritabanı | Teknoloji | Erişim | Amaç |
|------------|-----------|--------|------|
| **Nebim V3** | Dapper | Read-Only | Satış, stok, ürün verileri |
| **AppDB** | EF Core | Read/Write | Kullanıcı, chat, hedef, ayarlar |

### 2.2 Connection String Yapısı

```json
// appsettings.json
{
  "ConnectionStrings": {
    "NebimConnection": "Server=nebim-server;Database=NebimV3;User Id=reader;Password=***;TrustServerCertificate=true;",
    "AppConnection": "Host=localhost;Database=NebimAI_App;Username=postgres;Password=***;"
  }
}
```

### 2.3 Bağlantı Kayıt (DI)

```csharp
// Program.cs
// Nebim bağlantısı (Dapper için)
builder.Services.AddScoped<IDbConnection>(sp => 
    new SqlConnection(builder.Configuration.GetConnectionString("NebimConnection")));

// App DbContext (EF Core için)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppConnection")));
```

---

## 3. Repository Pattern

### 3.1 Generic Repository Interface

```csharp
namespace DAL.Repositories.Interfaces;

/// <summary>
/// Generic read repository - Nebim için
/// </summary>
public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic CRUD repository - AppDB için
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    // Read
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    // Write
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Query
    IQueryable<TEntity> Query();
}
```

### 3.2 Temel Kurallar

```csharp
// ✅ Doğru: Repository sadece data access işlemi yapar
public class UserRepository : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}

// ❌ Yanlış: Repository'de iş mantığı OLMAMALI
public class UserRepository : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        
        // ❌ YANLIŞ! İş mantığı BLL'de olmalı
        if (user != null && user.Role == UserRole.Admin)
        {
            user.Permissions = await GetAdminPermissions();
        }
        
        return user;
    }
}
```

---

## 4. Nebim Repository (Dapper)

### 4.1 Interface Tanımı

```csharp
namespace DAL.Repositories.Interfaces;

/// <summary>
/// Nebim V3 veritabanı okuma işlemleri
/// </summary>
public interface INebimRepository
{
    // Dashboard
    Task<DailySalesSummaryDto> GetTodaySalesAsync(CancellationToken ct = default);
    Task<IEnumerable<WeeklySalesDto>> GetWeeklySalesAsync(int weeks = 1, CancellationToken ct = default);
    Task<DashboardKpiDto> GetDashboardKpiAsync(CancellationToken ct = default);
    
    // Products
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto?> GetProductByCodeAsync(string code, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(StockFilterRequest filter, CancellationToken ct = default);
    
    // Stock
    Task<IEnumerable<StockDto>> GetStockByProductIdAsync(int productId, CancellationToken ct = default);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold, CancellationToken ct = default);
    
    // Categories & Seasons
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IEnumerable<SeasonDto>> GetSeasonsAsync(CancellationToken ct = default);
}
```

### 4.2 Dapper Implementasyonu

```csharp
namespace DAL.Repositories.Nebim;

public class NebimRepository : INebimRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<NebimRepository> _logger;

    public NebimRepository(IDbConnection connection, ILogger<NebimRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<DailySalesSummaryDto> GetTodaySalesAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT 
                CAST(GETDATE() AS DATE) AS SaleDate,
                ISNULL(SUM(TotalAmount), 0) AS TotalRevenue,
                COUNT(*) AS TransactionCount,
                ISNULL(SUM(CASE WHEN IsReturn = 1 THEN TotalAmount ELSE 0 END), 0) AS ReturnAmount
            FROM dbo.SalesTransactions
            WHERE CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)
                AND IsDeleted = 0";

        _logger.LogDebug("Executing GetTodaySalesAsync query");
        
        return await _connection.QueryFirstOrDefaultAsync<DailySalesSummaryDto>(
            new CommandDefinition(sql, cancellationToken: ct)) 
            ?? new DailySalesSummaryDto();
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(
        StockFilterRequest filter, 
        CancellationToken ct = default)
    {
        // ✅ Parametreli sorgu - SQL Injection koruması
        var parameters = new DynamicParameters();
        
        var whereClause = "WHERE p.IsDeleted = 0";
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            whereClause += " AND (p.ProductName LIKE @SearchTerm OR p.ProductCode LIKE @SearchTerm)";
            parameters.Add("SearchTerm", $"%{filter.SearchTerm}%");
        }
        
        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
        {
            whereClause += " AND c.CategoryCode = @CategoryCode";
            parameters.Add("CategoryCode", filter.CategoryCode);
        }

        // Count query
        var countSql = $@"
            SELECT COUNT(*) 
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
            {whereClause}";

        var totalCount = await _connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        // Data query with pagination
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var dataSql = $@"
            SELECT 
                p.Id,
                p.ProductCode AS Code,
                p.ProductName AS Name,
                c.CategoryName,
                s.SeasonCode,
                p.Price,
                ISNULL(st.TotalStock, 0) AS TotalStock,
                p.MinStock
            FROM dbo.Products p
            LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
            LEFT JOIN dbo.Seasons s ON p.SeasonId = s.Id
            LEFT JOIN (
                SELECT ProductId, SUM(Quantity) AS TotalStock 
                FROM dbo.Stocks 
                GROUP BY ProductId
            ) st ON p.Id = st.ProductId
            {whereClause}
            ORDER BY p.ProductName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = await _connection.QueryAsync<ProductListItemDto>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct));

        return PagedResult<ProductListItemDto>.Create(
            items.ToList(), 
            totalCount, 
            filter.Page, 
            filter.PageSize);
    }
}
```

### 4.3 Mock Nebim Repository

```csharp
namespace DAL.Repositories.Nebim;

/// <summary>
/// Geliştirme ve test için mock Nebim repository
/// </summary>
public class MockNebimRepository : INebimRepository
{
    private readonly ILogger<MockNebimRepository> _logger;

    public MockNebimRepository(ILogger<MockNebimRepository> logger)
    {
        _logger = logger;
    }

    public Task<DailySalesSummaryDto> GetTodaySalesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("MockNebimRepository: GetTodaySalesAsync called");
        
        return Task.FromResult(new DailySalesSummaryDto
        {
            SaleDate = DateTime.Today,
            TotalRevenue = 125750.00m,
            TransactionCount = 342,
            ReturnAmount = 3250.00m
        });
    }

    public Task<PagedResult<ProductListItemDto>> GetProductsAsync(
        StockFilterRequest filter, 
        CancellationToken ct = default)
    {
        var mockProducts = GenerateMockProducts(100);
        
        // Filtreleme
        var filtered = mockProducts.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Code.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(filter.CategoryCode))
        {
            filtered = filtered.Where(p => p.CategoryName == filter.CategoryCode);
        }

        var totalCount = filtered.Count();
        var items = filtered
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(PagedResult<ProductListItemDto>.Create(
            items, totalCount, filter.Page, filter.PageSize));
    }

    private static List<ProductListItemDto> GenerateMockProducts(int count)
    {
        var categories = new[] { "Üst Giyim", "Alt Giyim", "Dış Giyim", "Aksesuar" };
        var seasons = new[] { "2025 Kış", "2025 İlkbahar", "2024 Yaz" };
        var random = new Random(42); // Seed for consistent data

        return Enumerable.Range(1, count).Select(i => new ProductListItemDto
        {
            Id = i,
            Code = $"PRD-{i:D5}",
            Name = $"Ürün {i}",
            CategoryName = categories[random.Next(categories.Length)],
            SeasonCode = seasons[random.Next(seasons.Length)],
            Price = random.Next(50, 500) * 10m,
            TotalStock = random.Next(0, 200),
            MinStock = 20
        }).ToList();
    }
}
```

### 4.4 Dapper Kuralları

```csharp
// ✅ Doğru: Parametreli sorgu (SQL Injection koruması)
var sql = "SELECT * FROM Products WHERE CategoryId = @CategoryId";
var products = await _connection.QueryAsync<ProductDto>(sql, new { CategoryId = categoryId });

// ❌ Yanlış: String concatenation (SQL Injection riski!)
var sql = $"SELECT * FROM Products WHERE CategoryId = {categoryId}";  // YANLIŞ!

// ✅ Doğru: IN clause için Dapper kullanımı
var ids = new[] { 1, 2, 3, 4, 5 };
var sql = "SELECT * FROM Products WHERE Id IN @Ids";
var products = await _connection.QueryAsync<ProductDto>(sql, new { Ids = ids });

// ✅ Doğru: Transaction kullanımı (sadece gerektiğinde)
using var transaction = _connection.BeginTransaction();
try
{
    await _connection.ExecuteAsync(sql1, param1, transaction);
    await _connection.ExecuteAsync(sql2, param2, transaction);
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## 5. App Repository (EF Core)

### 5.1 DbContext Tanımı

```csharp
namespace DAL.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Target> Targets => Set<Target>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm konfigürasyonları otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Otomatik audit field güncelleme
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Soft delete işleme
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

### 5.2 Entity Configuration

```csharp
namespace DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.Role)
            .HasConversion<string>()  // Enum'u string olarak sakla
            .HasMaxLength(20);
        
        // Soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);
        
        // Navigation
        builder.HasMany(u => u.ChatSessions)
            .WithOne(cs => cs.User)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnType("text");  // PostgreSQL text type
        
        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(m => m.DataJson)
            .HasColumnType("jsonb");  // PostgreSQL JSONB
        
        builder.HasIndex(m => m.SessionId);
        builder.HasIndex(m => m.Timestamp);
    }
}
```

### 5.3 Repository Implementasyonu

```csharp
namespace DAL.Repositories.App;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.ToLower();
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLower(), ct);
    }

    public IQueryable<User> Query() => _context.Users.AsQueryable();
}
```

---

## 6. N+1 Query Problemi ve Çözümleri

### 6.1 Problem Tanımı

```csharp
// ❌ N+1 PROBLEM: Her session için ayrı query çalışır
public async Task<IEnumerable<ChatSessionDto>> GetSessionsWithMessagesAsync()
{
    var sessions = await _context.ChatSessions.ToListAsync();  // 1 query
    
    foreach (var session in sessions)
    {
        // Her session için +1 query = N+1 problem!
        session.Messages = await _context.ChatMessages
            .Where(m => m.SessionId == session.Id)
            .ToListAsync();
    }
    
    return sessions.Select(MapToDto);
}
```

### 6.2 Çözüm 1: Eager Loading (Include)

```csharp
// ✅ Doğru: Include ile tek sorguda getir
public async Task<IEnumerable<ChatSessionDto>> GetSessionsWithMessagesAsync()
{
    var sessions = await _context.ChatSessions
        .Include(s => s.Messages)  // LEFT JOIN ile tek sorgu
        .Include(s => s.User)
        .ToListAsync();
    
    return sessions.Select(MapToDto);
}

// ✅ Doğru: Filtered Include (EF Core 5+)
public async Task<ChatSession?> GetSessionWithRecentMessagesAsync(Guid sessionId)
{
    return await _context.ChatSessions
        .Include(s => s.Messages
            .Where(m => m.Timestamp > DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(m => m.Timestamp)
            .Take(50))
        .FirstOrDefaultAsync(s => s.Id == sessionId);
}
```

### 6.3 Çözüm 2: Explicit Loading

```csharp
// ✅ Koşullu yükleme gerektiğinde
public async Task<ChatSession?> GetSessionAsync(Guid id, bool includeMessages = false)
{
    var session = await _context.ChatSessions
        .FirstOrDefaultAsync(s => s.Id == id);
    
    if (session != null && includeMessages)
    {
        await _context.Entry(session)
            .Collection(s => s.Messages)
            .LoadAsync();
    }
    
    return session;
}
```

### 6.4 Çözüm 3: Projection (Select)

```csharp
// ✅ En iyi performans: Sadece gerekli alanları çek
public async Task<IEnumerable<ChatSessionSummaryDto>> GetSessionSummariesAsync(Guid userId)
{
    return await _context.ChatSessions
        .Where(s => s.UserId == userId)
        .Select(s => new ChatSessionSummaryDto
        {
            Id = s.Id,
            Title = s.Title,
            CreatedAt = s.CreatedAt,
            MessageCount = s.Messages.Count,  // Subquery, entity yüklenmez
            LastMessageAt = s.Messages
                .OrderByDescending(m => m.Timestamp)
                .Select(m => m.Timestamp)
                .FirstOrDefault()
        })
        .OrderByDescending(s => s.LastMessageAt)
        .ToListAsync();
}
```

### 6.5 Çözüm 4: Split Query

```csharp
// ✅ Çok büyük veri setleri için (Cartesian explosion önleme)
public async Task<IEnumerable<ChatSession>> GetSessionsWithAllDataAsync()
{
    return await _context.ChatSessions
        .Include(s => s.Messages)
        .Include(s => s.User)
        .AsSplitQuery()  // Ayrı sorgular çalıştır, tek bağlantıda
        .ToListAsync();
}
```

### 6.6 N+1 Kontrol Listesi

```csharp
// ❌ TEHLİKE İŞARETLERİ - Bunları gördüğünde N+1 olabilir:

// 1. Loop içinde await
foreach (var item in items)
{
    await _context.Related.Where(r => r.ItemId == item.Id).ToListAsync();  // N+1!
}

// 2. Navigation property erişimi (lazy loading aktifse)
var sessions = await _context.ChatSessions.ToListAsync();
foreach (var s in sessions)
{
    var count = s.Messages.Count;  // Her erişimde query!
}

// 3. Select içinde navigation property
var result = sessions.Select(s => new 
{
    s.Id,
    MessageCount = s.Messages.Count  // Memory'de olduğu için sorun yok AMA
                                      // Lazy loading aktifse N+1!
});
```

---

## 7. Unit of Work Pattern

### 7.1 Interface

```csharp
namespace DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IChatRepository Chats { get; }
    ITargetRepository Targets { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### 7.2 Implementasyon

```csharp
namespace DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IUserRepository? _users;
    private IChatRepository? _chats;
    private ITargetRepository? _targets;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IChatRepository Chats => _chats ??= new ChatRepository(_context);
    public ITargetRepository Targets => _targets ??= new TargetRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

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

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

### 7.3 Kullanım (BLL'de)

```csharp
public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ChatSession> CreateSessionWithMessageAsync(
        Guid userId, 
        string firstMessage,
        CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        
        try
        {
            // Session oluştur
            var session = new ChatSession
            {
                UserId = userId,
                Title = firstMessage.Length > 50 
                    ? firstMessage[..50] + "..." 
                    : firstMessage
            };
            await _unitOfWork.Chats.CreateSessionAsync(session, ct);
            
            // İlk mesajı ekle
            var message = new ChatMessage
            {
                SessionId = session.Id,
                Role = MessageRole.User,
                Content = firstMessage
            };
            await _unitOfWork.Chats.AddMessageAsync(message, ct);
            
            await _unitOfWork.CommitTransactionAsync(ct);
            
            return session;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
```

---

## 8. Connection Yönetimi

### 8.1 Connection Pooling

```csharp
// ✅ Doğru: Connection'ı DI'dan al, using ile dispose etme
public class NebimRepository : INebimRepository
{
    private readonly IDbConnection _connection;  // DI tarafından yönetilir
    
    public NebimRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    // Connection'ı manuel açmaya gerek yok, Dapper otomatik açar
    public async Task<ProductDto?> GetProductAsync(int id)
    {
        return await _connection.QueryFirstOrDefaultAsync<ProductDto>(
            "SELECT * FROM Products WHERE Id = @Id", 
            new { Id = id });
    }
}

// ❌ Yanlış: Her seferinde yeni connection oluşturma
public async Task<ProductDto?> GetProductAsync(int id)
{
    using var connection = new SqlConnection(_connectionString);  // Her çağrıda yeni!
    await connection.OpenAsync();
    return await connection.QueryFirstOrDefaultAsync<ProductDto>(/*...*/);
}
```

### 8.2 Timeout Ayarları

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "NebimConnection": "Server=...;Connection Timeout=30;Command Timeout=60;..."
  }
}

// Veya query bazlı timeout
var result = await _connection.QueryAsync<ProductDto>(
    new CommandDefinition(
        sql, 
        parameters, 
        commandTimeout: 120,  // 2 dakika
        cancellationToken: ct));
```

---

## 📝 Kontrol Listesi

DAL kodu yazarken şunları kontrol et:

- [ ] Repository interface'i `I` prefix'i ile tanımlandı mı?
- [ ] Nebim repository sadece okuma işlemi mi yapıyor?
- [ ] Dapper sorgularında parametreli query kullanıldı mı?
- [ ] EF Core'da Include kullanıldığında N+1 oluşmuyor mu?
- [ ] SaveChanges tek noktada mı çağrılıyor (Unit of Work)?
- [ ] CancellationToken parametresi eklendi mi?
- [ ] Connection DI'dan mı alınıyor?
- [ ] Soft delete query filter uygulandı mı?
- [ ] Mock repository var mı (test için)?

---

*Son Güncelleme: 26 Aralık 2025*
