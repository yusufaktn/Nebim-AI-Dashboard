# 📋 Nebim Admin Dashboard - Proje Kuralları ve Standartları

Bu dosya, projenin tutarlılığını ve kalitesini korumak için tüm geliştiricilerin uyması gereken kuralları tanımlar.

---

## 🏗️ Proje Mimarisi

### Katman Yapısı (N-Tier Architecture)
```
API (Sunum Katmanı)
 ↓
BLL (İş Mantığı Katmanı)
 ↓
DAL (Veri Erişim Katmanı)
 ↓
Entity (Domain Modelleri)
```

### Katman Sorumlulukları

| Katman | Sorumluluk | Bağımlılık |
|--------|------------|------------|
| **Api** | HTTP endpoint'leri, request/response, middleware, auth | BLL, Entity |
| **BLL** | İş kuralları, servisler, AI orchestration, validasyon | DAL, Entity |
| **DAL** | Veri erişimi, repository pattern, EF Core, Dapper | Entity |
| **Entity** | Domain modelleri, DTO'lar, enum'lar, exception'lar | Hiçbiri |

### Bağımlılık Kuralı
- Üst katmanlar alt katmanlara bağımlı olabilir
- Alt katmanlar ÜST katmanlara ASLA bağımlı olamaz
- Entity katmanı hiçbir katmana bağımlı değildir

---

## 📝 Naming Conventions

### Genel Kurallar

| Eleman | Format | Örnek |
|--------|--------|-------|
| Class | PascalCase | `ProductService`, `UserRepository` |
| Interface | I + PascalCase | `IProductService`, `IRepository<T>` |
| Method | PascalCase + Async | `GetProductsAsync()`, `ValidateAsync()` |
| Property | PascalCase | `ProductName`, `IsActive` |
| Private Field | _camelCase | `_logger`, `_repository` |
| Parameter | camelCase | `productId`, `cancellationToken` |
| Constant | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `DEFAULT_PAGE_SIZE` |
| Boolean | is/has/can prefix | `IsActive`, `HasStock`, `CanEdit` |

### Dosya Organizasyonu

```
Entity/
├── App/              # Uygulama domain modelleri (User, Tenant, etc.)
├── Base/             # Base class'lar (BaseEntity, IEntity)
├── DTOs/             # Data Transfer Objects
│   ├── AI/           # AI sistemi DTO'ları
│   ├── Request/      # API request DTO'ları
│   └── Response/     # API response DTO'ları
├── Enums/            # Enum tanımları
├── Exceptions/       # Custom exception'lar
├── Models/           # Value object'ler, helper modeller
└── Nebim/            # Nebim ERP entegrasyon modelleri

BLL/
├── AI/               # AI iş zekası sistemi
│   ├── Capabilities/ # Modüler yetenekler
│   ├── Orchestrator/ # Sorgu orkestrasyon
│   ├── Planner/      # AI query planner
│   └── Validation/   # Validasyon katmanı
├── Extensions/       # DI extension'ları
├── Helpers/          # Utility sınıfları
├── Mappings/         # Object mapping
└── Services/         # İş servisleri
    └── Interfaces/   # Servis interface'leri

DAL/
├── Configurations/   # EF Core entity configurations
├── Context/          # DbContext ve tenant yönetimi
├── Data/             # Seed data
├── Extensions/       # DI extension'ları
├── Migrations/       # EF Core migrations
├── Providers/        # Repository factory'ler
├── Repositories/     # Repository implementasyonları
└── UnitOfWork/       # Unit of Work pattern

Api/
├── Common/           # Shared utilities (ApiResponse)
├── Controllers/      # API controller'ları
├── Extensions/       # DI extension'ları
├── Middleware/       # Custom middleware'ler
└── Properties/       # launchSettings
```

---

## 🔌 Veritabanı Stratejisi

### Dual Database Yaklaşımı

| Veritabanı | Teknoloji | Erişim | Amaç |
|------------|-----------|--------|------|
| **AppDB** | PostgreSQL + EF Core | Read/Write | Kullanıcılar, tenant'lar, chat, ayarlar |
| **Nebim V3** | SQL Server + Dapper | Read-Only | Satış, stok, ürün verileri (tenant başına) |

### Multi-Tenant Connection Yönetimi
- Her tenant kendi Nebim instance'ına bağlanır
- Connection string'ler AES-256 ile şifrelenir
- Simulation modu development/demo için kullanılır
- `ITenantConnectionManager` runtime'da connection çözer

---

## 🤖 AI İş Zekası Sistemi

### Temel Prensipler

```
❌ YANLIŞ: AI cevap üretir
✅ DOĞRU: AI sorguyu analiz eder, plan üretir, backend execute eder

Admin Sorusu
     ↓
[ AI Anlamlandırma (Planner) ]
     ↓
[ Sorgu Planı (JSON) ]
     ↓
[ Capability Router (Orchestrator) ]
     ↓
[ Modüler İş Kuralları (Capabilities) ]
     ↓
[ Gerçek Veri (Nebim/AppDB) ]
     ↓
[ Güvenli Cevap ]
```

### Capability Pattern
```csharp
public interface ICapability
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Task<CapabilityResult> ExecuteAsync(int tenantId, JsonElement parameters, CancellationToken ct);
}
```

### Kapsam Dışı Sorgular
- AI tanıyamadığı sorularda `Intent: OutOfScope` döner
- En yakın capability önerileri `SuggestedCapabilities[]` ile sunulur
- Asla tahmin veya uydurma cevap verilmez

---

## ✅ Kod Yazım Kuralları

### Async/Await
```csharp
// ✅ Doğru
public async Task<Product> GetProductAsync(int id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ❌ Yanlış - CancellationToken eksik
public async Task<Product> GetProductAsync(int id)
```

### Dependency Injection
```csharp
// ✅ Doğru - Interface üzerinden
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// ❌ Yanlış - Concrete class
public ProductService(ProductRepository repository)
```

### Exception Handling
```csharp
// Custom exception'ları kullan
throw new NotFoundException("Product", productId);
throw new ValidationException("Invalid date range");
throw new BusinessException("Insufficient stock");

// Generic exception KULLANMA
throw new Exception("Something went wrong"); // ❌
```

### Null Safety
```csharp
// ✅ Null check pattern
var product = await _repository.GetByIdAsync(id, ct)
    ?? throw new NotFoundException("Product", id);

// ✅ Nullable reference types kullan
public async Task<Product?> FindProductAsync(string code)
```

---

## 📊 API Response Formatı

### Standart Response Wrapper
```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### HTTP Status Kodları
| Durum | Kod | Exception |
|-------|-----|-----------|
| Başarılı | 200 | - |
| Oluşturuldu | 201 | - |
| Validation hatası | 400 | `ValidationException` |
| Yetkisiz | 401 | `UnauthorizedException` |
| Erişim engelli | 403 | `ForbiddenException` |
| Bulunamadı | 404 | `NotFoundException` |
| Çakışma | 409 | `ConflictException` |
| İş kuralı hatası | 422 | `BusinessException` |
| Rate limit | 429 | `QuotaExceededException` |
| Sunucu hatası | 500 | `Exception` |

---

## 📁 Dokümantasyon Kuralları

### Görev Tamamlama
Her major görev sonrası:
1. `Docs/Development/CHANGELOG.md` güncelle
2. `Docs/Development/tasks/YYYY-MM-DD_feature-name.md` oluştur

### Task Log Formatı
```markdown
# Feature: [Feature Adı]
**Tarih:** YYYY-MM-DD
**Geliştirici:** [İsim]

## Yapılanlar
- [ ] Liste halinde tamamlanan işler

## Kararlar
- Neden bu yaklaşım seçildi?

## Bilinen Limitasyonlar
- Varsa kısıtlamalar

## Sonraki Adımlar
- Devam edilecek işler
```

---

## 🔐 Güvenlik Kuralları

### Connection String Güvenliği
- Nebim connection string'leri her zaman encrypted saklanır
- Master key `appsettings`'te değil, environment variable'da tutulur
- Connection string'ler loglara ASLA yazılmaz

### Multi-Tenant İzolasyon
- Her request'te `TenantId` JWT'den çözümlenir
- Repository katmanında tenant filter uygulanır
- Cross-tenant data erişimi kesinlikle engellenir

### Input Validation
- Tüm input'lar validate edilir
- SQL Injection için parameterized query kullanılır
- Connection string format kontrolü yapılır

---

## 🔄 Git Workflow

### Commit Message Formatı
```
type(scope): description

Örnekler:
feat(ai): add GetSalesCapability
fix(dal): tenant connection leak
docs: update CHANGELOG
refactor(bll): extract validation logic
```

### Branch Naming
```
feature/ai-capability-system
bugfix/tenant-connection-issue
hotfix/security-patch
```

---

## 🧪 Test Kuralları

### Test Naming
```csharp
[Fact]
public async Task GetProductAsync_WhenProductExists_ReturnsProduct()

[Fact]
public async Task GetProductAsync_WhenProductNotFound_ThrowsNotFoundException()
```

### Test Coverage
- Service katmanı: %80+ coverage
- Capability'ler: %90+ coverage
- Repository: Integration test

---

## 📌 Önemli Notlar

1. **AI asla cevap üretmez** - Sadece query plan üretir
2. **Her capability bağımsızdır** - Tek sorumluluk prensibi
3. **Tenant izolasyonu kritiktir** - Cross-tenant erişim yoktur
4. **Simulation modu** - Development için, production'da gerçek Nebim
5. **Dokümantasyon zorunludur** - Her görev loglanır

---

*Son güncelleme: 2024-12-28*
