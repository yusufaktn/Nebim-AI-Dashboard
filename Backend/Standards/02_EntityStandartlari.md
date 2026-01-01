# 📦 Entity Katmanı Standartları

> Bu doküman, Entity katmanındaki Model, DTO ve Enum tasarım standartlarını tanımlar.

---

## 📌 İçindekiler

1. [Klasör Yapısı](#1-klasör-yapısı)
2. [Model vs DTO Ayrımı](#2-model-vs-dto-ayrımı)
3. [Nebim DTO Standartları](#3-nebim-dto-standartları)
4. [App Entity Standartları](#4-app-entity-standartları)
5. [Enum Standartları](#5-enum-standartları)
6. [FluentValidation Kuralları](#6-fluentvalidation-kuralları)
7. [Request/Response DTO'ları](#7-requestresponse-dtoları)

---

## 1. Klasör Yapısı

```
Entity/
├── Models/
│   ├── Nebim/              # Nebim V3'ten gelen veriler için DTO'lar (Read-Only)
│   │   ├── ProductDto.cs
│   │   ├── SalesDto.cs
│   │   ├── StockDto.cs
│   │   └── ...
│   │
│   └── App/                # AppDB entity'leri (EF Core ile yönetilen)
│       ├── User.cs
│       ├── ChatSession.cs
│       ├── ChatMessage.cs
│       ├── Target.cs
│       └── AppSetting.cs
│
├── DTOs/
│   ├── Request/            # API'ye gelen istekler
│   │   ├── CreateUserRequest.cs
│   │   ├── SendMessageRequest.cs
│   │   └── StockFilterRequest.cs
│   │
│   └── Response/           # API'den dönen cevaplar
│       ├── DashboardResponse.cs
│       ├── ProductListResponse.cs
│       └── ChatResponse.cs
│
├── Enums/
│   ├── UserRole.cs
│   ├── StockStatus.cs
│   ├── MessageRole.cs
│   └── ...
│
└── Common/
    ├── BaseEntity.cs       # Ortak entity özellikleri
    ├── IAuditableEntity.cs # Created/Updated tracking
    └── PagedResult.cs      # Sayfalama için generic wrapper
```

---

## 2. Model vs DTO Ayrımı

### 2.1 Temel Farklar

| Özellik | Entity (Model) | DTO |
|---------|----------------|-----|
| **Amaç** | Veritabanı tablosunu temsil eder | Veri transferi için kullanılır |
| **Konum** | `Models/App/` | `Models/Nebim/`, `DTOs/` |
| **EF Core** | DbSet olarak tanımlanır | Tanımlanmaz |
| **Navigation Property** | Olabilir | Olmamalı |
| **Validation** | Minimal | FluentValidation ile kapsamlı |
| **Değişiklik** | Migration gerektirir | Serbestçe değiştirilebilir |

### 2.2 Entity Örneği (AppDB)

```csharp
// ✅ Doğru: App Entity - EF Core tarafından yönetilir
namespace Entity.Models.App;

public class User : BaseEntity, IAuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string? AvatarUrl { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ChatSession> ChatSessions { get; set; } = [];
}
```

### 2.3 DTO Örneği (Nebim)

```csharp
// ✅ Doğru: Nebim DTO - Sadece veri taşıma amaçlı
namespace Entity.Models.Nebim;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SeasonCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int TotalStock { get; set; }
    public int MinStock { get; set; }
    public StockStatus Status { get; set; }
    
    // ❌ Navigation property OLMAMALI
    // public Category Category { get; set; }  // YANLIŞ!
}
```

---

## 3. Nebim DTO Standartları

### 3.1 Genel Kurallar

```csharp
// ✅ Doğru: Nebim DTO tasarımı
namespace Entity.Models.Nebim;

/// <summary>
/// Nebim V3 satış özeti - Sadece okuma amaçlı
/// </summary>
public class DailySalesDto
{
    /// <summary>Satış tarihi</summary>
    public DateTime SaleDate { get; set; }
    
    /// <summary>Toplam ciro (KDV dahil)</summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>Satış adedi</summary>
    public int TransactionCount { get; set; }
    
    /// <summary>İade tutarı</summary>
    public decimal ReturnAmount { get; set; }
    
    /// <summary>Net satış tutarı</summary>
    public decimal NetSales => TotalRevenue - ReturnAmount;
}
```

### 3.2 Suffix Kuralları

| Suffix | Kullanım | Örnek |
|--------|----------|-------|
| `Dto` | Nebim'den gelen veri | `ProductDto`, `SalesDto` |
| `SummaryDto` | Özet veri | `DailySalesSummaryDto` |
| `DetailDto` | Detaylı veri | `ProductDetailDto` |
| `ListItemDto` | Liste görünümü | `ProductListItemDto` |

### 3.3 Computed Property Kullanımı

```csharp
public class StockDto
{
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    
    // ✅ Computed property - veritabanında yok, hesaplanıyor
    public StockStatus Status => CurrentStock switch
    {
        0 => StockStatus.OutOfStock,
        var s when s <= MinStock => StockStatus.LowStock,
        var s when s >= MaxStock => StockStatus.OverStock,
        _ => StockStatus.Normal
    };
    
    public decimal StockPercentage => MaxStock > 0 
        ? Math.Round((decimal)CurrentStock / MaxStock * 100, 2) 
        : 0;
}
```

---

## 4. App Entity Standartları

### 4.1 BaseEntity Kullanımı

```csharp
// Common/BaseEntity.cs
namespace Entity.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

// Common/IAuditableEntity.cs
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

// Common/ISoftDeletable.cs
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
```

### 4.2 Entity Örneği

```csharp
namespace Entity.Models.App;

public class ChatSession : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
```

### 4.3 Navigation Property Kuralları

```csharp
// ✅ Doğru: Required navigation property
public User User { get; set; } = null!;  // null-forgiving operator

// ✅ Doğru: Optional navigation property
public User? AssignedUser { get; set; }

// ✅ Doğru: Collection navigation property
public ICollection<ChatMessage> Messages { get; set; } = [];

// ❌ Yanlış: List kullanma
public List<ChatMessage> Messages { get; set; }  // ICollection tercih et
```

### 4.4 Primary Key Stratejisi

```csharp
// ✅ Doğru: GUID kullan (distributed system uyumlu)
public class User : BaseEntity  // BaseEntity'de Guid Id var
{
    // Id otomatik olarak gelir
}

// Alternatif: Sequential GUID (SQL Server için optimize)
public Guid Id { get; set; } = Guid.CreateVersion7();  // .NET 9+
```

---

## 5. Enum Standartları

### 5.1 Enum Tanımlama

```csharp
namespace Entity.Enums;

/// <summary>
/// Kullanıcı yetki seviyeleri
/// </summary>
public enum UserRole
{
    /// <summary>Sadece görüntüleme yetkisi</summary>
    Viewer = 0,
    
    /// <summary>Standart kullanıcı</summary>
    User = 1,
    
    /// <summary>Yönetici - Tam yetki</summary>
    Admin = 2
}

/// <summary>
/// Stok durumu göstergesi
/// </summary>
public enum StockStatus
{
    OutOfStock = 0,    // Stok yok
    LowStock = 1,      // Kritik seviye
    Normal = 2,        // Normal
    OverStock = 3      // Fazla stok
}

/// <summary>
/// Chat mesaj rolü
/// </summary>
public enum MessageRole
{
    User = 0,
    Assistant = 1,
    System = 2
}
```

### 5.2 Enum Kullanım Kuralları

```csharp
// ✅ Doğru: Explicit değer ataması (API uyumluluğu için)
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

// ✅ Doğru: Flags attribute (birden fazla değer seçilebilir)
[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Admin = Read | Write | Delete  // 7
}

// ❌ Yanlış: String karşılaştırma
if (user.Role.ToString() == "Admin")  // YANLIŞ!

// ✅ Doğru: Enum karşılaştırma
if (user.Role == UserRole.Admin)
```

---

## 6. FluentValidation Kuralları

### 6.1 Validator Yapısı

```csharp
using FluentValidation;

namespace Entity.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir");
        
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçersiz kullanıcı rolü");
    }
}
```

### 6.2 Özel Validation Kuralları

```csharp
public class StockFilterRequestValidator : AbstractValidator<StockFilterRequest>
{
    public StockFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1'den küçük olamaz");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1-100 arasında olmalıdır");
        
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
            .WithMessage("Minimum fiyat 0'dan küçük olamaz");
        
        RuleFor(x => x.MaxPrice)
            .GreaterThan(x => x.MinPrice ?? 0).When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue)
            .WithMessage("Maksimum fiyat, minimum fiyattan büyük olmalıdır");
        
        // Özel kural
        RuleFor(x => x.DateRange)
            .Must(BeValidDateRange).When(x => x.DateRange != null)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
    }
    
    private bool BeValidDateRange(DateRangeDto? range)
    {
        if (range == null) return true;
        return range.EndDate >= range.StartDate;
    }
}
```

### 6.3 Validator Kayıt (DI)

```csharp
// Program.cs veya ayrı extension method
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

// Veya manuel kayıt
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
```

---

## 7. Request/Response DTO'ları

### 7.1 Request DTO

```csharp
namespace Entity.DTOs.Request;

/// <summary>
/// Yeni kullanıcı oluşturma isteği
/// </summary>
public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}

/// <summary>
/// Stok filtreleme isteği
/// </summary>
public class StockFilterRequest
{
    public string? SearchTerm { get; set; }
    public string? CategoryCode { get; set; }
    public string? SeasonCode { get; set; }
    public StockStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// AI chat mesaj gönderme isteği
/// </summary>
public class SendMessageRequest
{
    public Guid? SessionId { get; set; }  // Null ise yeni session oluşturulur
    public string Message { get; set; } = string.Empty;
}
```

### 7.2 Response DTO

```csharp
namespace Entity.DTOs.Response;

/// <summary>
/// Dashboard özet verisi
/// </summary>
public class DashboardResponse
{
    public List<KpiItemDto> KpiCards { get; set; } = [];
    public List<SalesChartDataDto> WeeklySales { get; set; } = [];
    public List<AiSuggestionDto> AiSuggestions { get; set; } = [];
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sayfalanmış ürün listesi
/// </summary>
public class ProductListResponse
{
    public List<ProductListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// AI chat yanıtı
/// </summary>
public class ChatResponse
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public MessageRole Role { get; set; } = MessageRole.Assistant;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ChatDataDto? Data { get; set; }  // Opsiyonel: Tablo, grafik verisi
}
```

### 7.3 Ortak Wrapper: PagedResult

```csharp
namespace Entity.Common;

/// <summary>
/// Generic sayfalama wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    
    public int TotalPages => PageSize > 0 
        ? (int)Math.Ceiling((double)TotalCount / PageSize) 
        : 0;
    
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    
    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
```

---

## 📝 Kontrol Listesi

Entity/DTO oluştururken şunları kontrol et:

- [ ] Doğru namespace kullanıldı mı? (`Entity.Models.Nebim`, `Entity.Models.App`, `Entity.DTOs.Request`)
- [ ] Nebim DTO'larında navigation property yok mu?
- [ ] App Entity'lerde `BaseEntity` ve `IAuditableEntity` implement edildi mi?
- [ ] Nullable tipler `?` ile işaretlendi mi?
- [ ] String property'ler default değer aldı mı? (`= string.Empty`)
- [ ] Collection property'ler initialize edildi mi? (`= []`)
- [ ] FluentValidation validator'ı yazıldı mı?
- [ ] XML documentation eklendi mi?
- [ ] Enum değerleri explicit sayı aldı mı?

---

*Son Güncelleme: 26 Aralık 2025*
