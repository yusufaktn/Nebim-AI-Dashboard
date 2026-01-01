# 📋 Genel Kodlama Standartları

> Bu doküman, Nebim Dashboard Backend projesinin tüm katmanlarında uyulması gereken temel kodlama standartlarını tanımlar.

---

## 📌 İçindekiler

1. [Naming Convention (İsimlendirme Kuralları)](#1-naming-convention-i̇simlendirme-kuralları)
2. [SOLID Prensipleri](#2-solid-prensipleri)
3. [DRY Prensibi](#3-dry-prensibi)
4. [Async/Await Kuralları](#4-asyncawait-kuralları)
5. [Nullable Referans Tipler](#5-nullable-referans-tipler)
6. [Genel Kod Yazım Kuralları](#6-genel-kod-yazım-kuralları)

---

## 1. Naming Convention (İsimlendirme Kuralları)

### 1.1 Genel Kurallar

| Tür | Kural | Örnek |
|-----|-------|-------|
| **Class** | PascalCase | `ProductService`, `UserRepository` |
| **Interface** | I + PascalCase | `IProductService`, `IUserRepository` |
| **Method** | PascalCase + Fiil | `GetProducts()`, `CreateUser()`, `UpdateStock()` |
| **Property** | PascalCase | `FirstName`, `CreatedAt`, `IsActive` |
| **Private Field** | _camelCase | `_productRepository`, `_logger` |
| **Parameter** | camelCase | `productId`, `userName`, `pageSize` |
| **Local Variable** | camelCase | `totalAmount`, `filteredProducts` |
| **Constant** | PascalCase | `MaxPageSize`, `DefaultTimeout` |
| **Enum** | PascalCase (tekil) | `UserRole`, `StockStatus` |
| **Enum Value** | PascalCase | `UserRole.Admin`, `StockStatus.OutOfStock` |

### 1.2 Dosya İsimlendirme

```
✅ Doğru:
ProductService.cs
IProductRepository.cs
CreateProductDto.cs
UserRole.cs

❌ Yanlış:
productService.cs
Product_Service.cs
product-service.cs
```

### 1.3 Async Method İsimlendirme

```csharp
// ✅ Doğru: Async suffix kullan
public async Task<Product> GetProductByIdAsync(int id)
public async Task CreateUserAsync(CreateUserDto dto)

// ❌ Yanlış: Async suffix eksik
public async Task<Product> GetProductById(int id)
```

### 1.4 Boolean İsimlendirme

```csharp
// ✅ Doğru: is, has, can, should prefix'leri
public bool IsActive { get; set; }
public bool HasStock { get; set; }
public bool CanEdit { get; set; }

// ❌ Yanlış
public bool Active { get; set; }
public bool Stock { get; set; }
```

---

## 2. SOLID Prensipleri

### 2.1 Single Responsibility (Tek Sorumluluk)

Her sınıf sadece bir iş yapmalı, sadece bir değişiklik sebebi olmalı.

```csharp
// ❌ Yanlış: Birden fazla sorumluluk
public class ProductService
{
    public Product GetProduct(int id) { /* ... */ }
    public void SendEmail(string to, string subject) { /* ... */ }  // Email gönderme burada olmamalı!
    public void GeneratePdfReport() { /* ... */ }  // Rapor üretme burada olmamalı!
}

// ✅ Doğru: Tek sorumluluk
public class ProductService
{
    public Task<Product> GetProductAsync(int id) { /* ... */ }
    public Task<IEnumerable<Product>> GetProductsAsync() { /* ... */ }
}

public class EmailService
{
    public Task SendEmailAsync(string to, string subject) { /* ... */ }
}

public class ReportService
{
    public Task<byte[]> GeneratePdfReportAsync() { /* ... */ }
}
```

### 2.2 Open/Closed (Açık/Kapalı)

Sınıflar genişletmeye açık, değişikliğe kapalı olmalı.

```csharp
// ✅ Doğru: Interface ile genişletilebilir yapı
public interface INotificationService
{
    Task SendAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public Task SendAsync(string message) => /* email gönder */;
}

public class SmsNotificationService : INotificationService
{
    public Task SendAsync(string message) => /* sms gönder */;
}
```

### 2.3 Liskov Substitution (Liskov Yerine Geçme)

Alt sınıflar, üst sınıfların yerine kullanılabilmeli.

```csharp
// ✅ Doğru: INebimRepository yerine MockNebimRepository kullanılabilir
public interface INebimRepository
{
    Task<IEnumerable<ProductDto>> GetProductsAsync();
}

public class NebimRepository : INebimRepository { /* Gerçek Dapper implementasyonu */ }
public class MockNebimRepository : INebimRepository { /* Mock data */ }
```

### 2.4 Interface Segregation (Arayüz Ayrımı)

Büyük interface'ler yerine küçük, özelleşmiş interface'ler kullan.

```csharp
// ❌ Yanlış: Çok büyük interface
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<byte[]> ExportToPdfAsync();  // Herkes PDF export istemez!
    Task SendNotificationAsync();      // Herkes bildirim istemez!
}

// ✅ Doğru: Ayrılmış interface'ler
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### 2.5 Dependency Inversion (Bağımlılık Tersine Çevirme)

Üst seviye modüller, alt seviye modüllere değil, soyutlamalara (interface) bağımlı olmalı.

```csharp
// ❌ Yanlış: Concrete class'a bağımlılık
public class ProductService
{
    private readonly NebimRepository _repository = new NebimRepository();
}

// ✅ Doğru: Interface'e bağımlılık + Constructor Injection
public class ProductService : IProductService
{
    private readonly INebimRepository _nebimRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        INebimRepository nebimRepository,
        ILogger<ProductService> logger)
    {
        _nebimRepository = nebimRepository;
        _logger = logger;
    }
}
```

---

## 3. DRY Prensibi

**Don't Repeat Yourself** - Kendini tekrar etme!

### 3.1 Ortak Kod Çıkarma

```csharp
// ❌ Yanlış: Kod tekrarı
public async Task<ProductDto> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    if (product == null)
        throw new NotFoundException($"Product with id {id} not found");
    return product;
}

public async Task<CategoryDto> GetCategoryAsync(int id)
{
    var category = await _repository.GetByIdAsync(id);
    if (category == null)
        throw new NotFoundException($"Category with id {id} not found");
    return category;
}

// ✅ Doğru: Extension method ile ortak kod
public static class EntityExtensions
{
    public static T EnsureFound<T>(this T? entity, string entityName, object id) where T : class
    {
        if (entity == null)
            throw new NotFoundException($"{entityName} with id {id} not found");
        return entity;
    }
}

// Kullanım:
var product = (await _repository.GetByIdAsync(id)).EnsureFound("Product", id);
```

### 3.2 Magic String/Number Yasağı

```csharp
// ❌ Yanlış: Magic string ve number
if (user.Role == "admin") { }
if (stock < 10) { }
var timeout = 30000;

// ✅ Doğru: Constant veya Enum kullan
public static class AppConstants
{
    public const int LowStockThreshold = 10;
    public const int DefaultTimeoutMs = 30000;
    public const int MaxPageSize = 100;
}

public enum UserRole
{
    Admin,
    User,
    Viewer
}

// Kullanım:
if (user.Role == UserRole.Admin) { }
if (stock < AppConstants.LowStockThreshold) { }
```

---

## 4. Async/Await Kuralları

### 4.1 Temel Kurallar

```csharp
// ✅ Doğru: async/await pattern
public async Task<ProductDto> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    return _mapper.Map<ProductDto>(product);
}

// ❌ Yanlış: .Result veya .Wait() kullanımı (Deadlock riski!)
public ProductDto GetProduct(int id)
{
    var product = _repository.GetByIdAsync(id).Result;  // YANLIŞ!
    return _mapper.Map<ProductDto>(product);
}

// ❌ Yanlış: async void (sadece event handler'larda kullanılabilir)
public async void ProcessOrder() { }  // YANLIŞ!

// ✅ Doğru: async Task
public async Task ProcessOrderAsync() { }
```

### 4.2 Paralel İşlemler

```csharp
// ✅ Doğru: Bağımsız işlemleri paralel çalıştır
public async Task<DashboardDto> GetDashboardAsync()
{
    var salesTask = _nebimRepository.GetTodaySalesAsync();
    var stockTask = _nebimRepository.GetLowStockCountAsync();
    var ordersTask = _nebimRepository.GetPendingOrdersAsync();

    await Task.WhenAll(salesTask, stockTask, ordersTask);

    return new DashboardDto
    {
        TodaySales = await salesTask,
        LowStockCount = await stockTask,
        PendingOrders = await ordersTask
    };
}

// ❌ Yanlış: Sıralı bekleme (yavaş!)
public async Task<DashboardDto> GetDashboardAsync()
{
    var sales = await _nebimRepository.GetTodaySalesAsync();
    var stock = await _nebimRepository.GetLowStockCountAsync();  // Sales bitmeden başlamıyor!
    var orders = await _nebimRepository.GetPendingOrdersAsync(); // Stock bitmeden başlamıyor!
    // ...
}
```

### 4.3 CancellationToken Kullanımı

```csharp
// ✅ Doğru: CancellationToken kabul et ve ilet
public async Task<IEnumerable<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
{
    return await _repository.GetAllAsync(cancellationToken);
}

// Controller'da:
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(CancellationToken cancellationToken)
{
    var products = await _productService.GetProductsAsync(cancellationToken);
    return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
}
```

---

## 5. Nullable Referans Tipler

### 5.1 Proje Ayarı

```xml
<!-- .csproj dosyasında -->
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

### 5.2 Nullable Kullanımı

```csharp
// ✅ Doğru: Nullable olabilecek değerleri işaretle
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Non-null, default değer
    public string? Description { get; set; }           // Nullable
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }           // Nullable DateTime
}

// ✅ Doğru: Null kontrolü
public async Task<ProductDto?> GetProductByIdAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    return product;  // null dönebilir
}

// Kullanım:
var product = await _service.GetProductByIdAsync(id);
if (product is null)
{
    throw new NotFoundException("Product not found");
}
```

### 5.3 Null-Conditional ve Null-Coalescing

```csharp
// ✅ Doğru: Null-conditional operator
var userName = user?.Profile?.DisplayName;

// ✅ Doğru: Null-coalescing operator
var displayName = user?.DisplayName ?? "Anonymous";

// ✅ Doğru: Null-coalescing assignment
user.DisplayName ??= "Default Name";
```

---

## 6. Genel Kod Yazım Kuralları

### 6.1 Var Kullanımı

```csharp
// ✅ Doğru: Tip açıkça belli olduğunda var kullan
var products = new List<Product>();
var user = await _repository.GetUserByIdAsync(id);
var count = products.Count;

// ✅ Doğru: Tip belli değilse explicit tip kullan
IEnumerable<Product> products = GetProducts();  // Dönüş tipi belirsizse
```

### 6.2 String İşlemleri

```csharp
// ✅ Doğru: String interpolation
var message = $"Product {productName} has {stockCount} items in stock";

// ✅ Doğru: String.IsNullOrEmpty / IsNullOrWhiteSpace
if (string.IsNullOrWhiteSpace(userName))
{
    throw new ValidationException("Username is required");
}

// ❌ Yanlış
if (userName == null || userName == "") { }
if (userName == "") { }
```

### 6.3 Collection Initialization

```csharp
// ✅ Doğru: Collection expression (C# 12+)
List<string> names = ["Ali", "Veli", "Ayşe"];
int[] numbers = [1, 2, 3, 4, 5];

// ✅ Doğru: Object initializer
var product = new Product
{
    Name = "T-Shirt",
    Price = 199.99m,
    IsActive = true
};
```

### 6.4 LINQ Best Practices

```csharp
// ✅ Doğru: Method syntax (tercih edilen)
var activeProducts = products
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
    .ToList();

// ✅ Doğru: Any() vs Count() > 0
if (products.Any())  // Daha performanslı
if (products.Any(p => p.IsActive))

// ❌ Yanlış
if (products.Count() > 0)  // Tüm listeyi sayar!

// ✅ Doğru: FirstOrDefault vs Single
var product = products.FirstOrDefault(p => p.Id == id);  // Bulamazsa null
var product = products.SingleOrDefault(p => p.Id == id); // Birden fazla varsa exception
```

### 6.5 Region Kullanımı

```csharp
// ❌ Yanlış: Region ile kod gizleme (code smell!)
#region Private Methods
// 500 satır kod...
#endregion

// ✅ Doğru: Sınıfı küçük tut, gerekirse ayır
// Eğer region ihtiyacı duyuyorsanız, sınıf çok büyük demektir!
```

### 6.6 Yorum Yazma Kuralları

```csharp
// ❌ Yanlış: Gereksiz yorum
// Get product by id
public async Task<Product> GetProductByIdAsync(int id)

// ✅ Doğru: Neden açıklayan yorum
// Nebim V3'te soft delete yok, bu yüzden IsActive flag'i kontrol ediyoruz
public async Task<Product?> GetActiveProductByIdAsync(int id)

// ✅ Doğru: XML documentation (public API'ler için)
/// <summary>
/// Belirtilen ID'ye sahip ürünü getirir.
/// </summary>
/// <param name="id">Ürün ID'si</param>
/// <returns>Ürün DTO'su veya bulunamazsa null</returns>
/// <exception cref="NotFoundException">Ürün bulunamadığında fırlatılır</exception>
public async Task<ProductDto?> GetProductByIdAsync(int id)
```

---

## 📝 Kontrol Listesi

Kod yazarken şu soruları sor:

- [ ] İsimlendirme kurallarına uyuyor mu?
- [ ] Async method'lar `Async` suffix'i ile mi bitiyor?
- [ ] `async void` kullanılmamış mı?
- [ ] `.Result` veya `.Wait()` kullanılmamış mı?
- [ ] Magic string/number var mı? Constant'a çevrilmeli mi?
- [ ] Kod tekrarı var mı? Ortak metod çıkarılabilir mi?
- [ ] Nullable tipler doğru işaretlenmiş mi?
- [ ] CancellationToken destekleniyor mu?
- [ ] Interface'e mi bağımlı yoksa concrete class'a mı?

---

*Son Güncelleme: 26 Aralık 2025*
