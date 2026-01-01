# ⚙️ BLL (Business Logic Layer) Standartları

> Bu doküman, Business Logic Layer katmanındaki Service tasarımı, iş mantığı kuralları, transaction yönetimi ve Semantic Kernel entegrasyonunu tanımlar.

---

## 📌 İçindekiler

1. [Klasör Yapısı](#1-klasör-yapısı)
2. [Service Tasarım Kuralları](#2-service-tasarım-kuralları)
3. [İş Mantığı Kuralları](#3-iş-mantığı-kuralları)
4. [Transaction Yönetimi](#4-transaction-yönetimi)
5. [Dependency Injection](#5-dependency-injection)
6. [Semantic Kernel (AI) Entegrasyonu](#6-semantic-kernel-ai-entegrasyonu)
7. [Caching Stratejisi](#7-caching-stratejisi)
8. [Mapping (DTO Dönüşümleri)](#8-mapping-dto-dönüşümleri)

---

## 1. Klasör Yapısı

```
BLL/
├── Services/
│   ├── Interfaces/
│   │   ├── IDashboardService.cs
│   │   ├── IStockService.cs
│   │   ├── IChatService.cs
│   │   ├── IUserService.cs
│   │   └── IAuthService.cs
│   │
│   ├── DashboardService.cs
│   ├── StockService.cs
│   ├── ChatService.cs
│   ├── UserService.cs
│   └── AuthService.cs
│
├── AI/
│   ├── ChatOrchestrator.cs          # Ana AI chat yönetimi
│   ├── Plugins/
│   │   ├── NebimQueryPlugin.cs      # Nebim veritabanı sorgulama
│   │   ├── StockAnalysisPlugin.cs   # Stok analizi
│   │   └── SalesReportPlugin.cs     # Satış raporları
│   └── Prompts/
│       └── SystemPrompt.txt         # AI sistem prompt'u
│
├── Validators/
│   ├── CreateUserValidator.cs
│   ├── SendMessageValidator.cs
│   └── StockFilterValidator.cs
│
├── Mappings/
│   └── MappingExtensions.cs         # Manuel mapping extension'ları
│
└── Helpers/
    ├── PasswordHelper.cs
    └── DateHelper.cs
```

---

## 2. Service Tasarım Kuralları

### 2.1 Interface Tanımlama

```csharp
namespace BLL.Services.Interfaces;

/// <summary>
/// Dashboard iş mantığı servisi
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Dashboard için KPI verilerini getirir
    /// </summary>
    Task<DashboardResponse> GetDashboardDataAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Haftalık satış grafiği verilerini getirir
    /// </summary>
    Task<IEnumerable<WeeklySalesDto>> GetWeeklySalesAsync(int weeks = 1, CancellationToken ct = default);
    
    /// <summary>
    /// AI tarafından oluşturulan önerileri getirir
    /// </summary>
    Task<IEnumerable<AiSuggestionDto>> GetAiSuggestionsAsync(CancellationToken ct = default);
}
```

### 2.2 Service Implementasyonu

```csharp
namespace BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly INebimRepository _nebimRepository;
    private readonly IChatOrchestrator _chatOrchestrator;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        INebimRepository nebimRepository,
        IChatOrchestrator chatOrchestrator,
        ILogger<DashboardService> logger)
    {
        _nebimRepository = nebimRepository;
        _chatOrchestrator = chatOrchestrator;
        _logger = logger;
    }

    public async Task<DashboardResponse> GetDashboardDataAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching dashboard data");

        // ✅ Paralel çağrılar - bağımsız işlemler
        var kpiTask = GetKpiDataAsync(ct);
        var salesTask = _nebimRepository.GetWeeklySalesAsync(1, ct);
        var suggestionsTask = GetAiSuggestionsAsync(ct);

        await Task.WhenAll(kpiTask, salesTask, suggestionsTask);

        return new DashboardResponse
        {
            KpiCards = await kpiTask,
            WeeklySales = (await salesTask).ToList(),
            AiSuggestions = (await suggestionsTask).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<List<KpiItemDto>> GetKpiDataAsync(CancellationToken ct)
    {
        var todaySales = await _nebimRepository.GetTodaySalesAsync(ct);
        var yesterdaySales = await _nebimRepository.GetSalesByDateAsync(DateTime.Today.AddDays(-1), ct);

        // ✅ İş mantığı burada - hesaplamalar BLL'de yapılır
        var revenueChange = CalculatePercentageChange(
            todaySales.TotalRevenue, 
            yesterdaySales.TotalRevenue);

        return new List<KpiItemDto>
        {
            new()
            {
                Id = "daily-revenue",
                Title = "Günün Cirosu",
                Value = todaySales.TotalRevenue.ToCurrencyString(),
                Change = revenueChange,
                ChangeType = revenueChange >= 0 ? ChangeType.Increase : ChangeType.Decrease,
                Icon = "payments",
                Color = "blue"
            },
            // ... diğer KPI'lar
        };
    }

    private static decimal CalculatePercentageChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / previous * 100, 2);
    }
}
```

### 2.3 Temel Kurallar

```csharp
// ✅ Doğru: Service tek bir sorumluluğa sahip
public class StockService : IStockService
{
    // Sadece stok ile ilgili işlemler
    public Task<PagedResult<ProductListItemDto>> GetProductsAsync(StockFilterRequest filter);
    public Task<ProductDetailDto> GetProductDetailAsync(int productId);
    public Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync();
}

// ❌ Yanlış: Tek service'te her şey
public class MegaService
{
    public Task<User> GetUser();
    public Task SendEmail();
    public Task<Product> GetProduct();
    public Task GenerateReport();
    // ... 50 tane daha method
}
```

---

## 3. İş Mantığı Kuralları

### 3.1 Service'te Sorgu Yapılmaz!

```csharp
// ❌ YANLIŞ: Service içinde raw SQL veya LINQ sorgusu
public class StockService : IStockService
{
    private readonly AppDbContext _context;  // ❌ DbContext doğrudan kullanılmamalı
    
    public async Task<IEnumerable<Product>> GetLowStockProducts()
    {
        // ❌ YANLIŞ! Bu sorgu DAL'da olmalı
        return await _context.Products
            .Where(p => p.Stock < p.MinStock)
            .ToListAsync();
    }
}

// ✅ DOĞRU: Repository'den veri al, iş mantığını uygula
public class StockService : IStockService
{
    private readonly INebimRepository _nebimRepository;
    
    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct)
    {
        // ✅ Repository'den veri al
        var products = await _nebimRepository.GetLowStockProductsAsync(ct);
        
        // ✅ İş mantığını uygula
        return products
            .Select(p => new LowStockAlertDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CurrentStock = p.Stock,
                MinStock = p.MinStock,
                Severity = CalculateSeverity(p.Stock, p.MinStock),
                SuggestedOrder = CalculateSuggestedOrder(p)
            })
            .OrderByDescending(a => a.Severity);
    }
    
    private static AlertSeverity CalculateSeverity(int current, int min)
    {
        var ratio = (double)current / min;
        return ratio switch
        {
            0 => AlertSeverity.Critical,
            < 0.25 => AlertSeverity.High,
            < 0.5 => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };
    }
}
```

### 3.2 Validation

```csharp
public class UserService : IUserService
{
    private readonly IValidator<CreateUserRequest> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        // ✅ FluentValidation ile doğrulama
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // ✅ İş kuralı kontrolü
        var exists = await _unitOfWork.Users.ExistsAsync(request.Email, ct);
        if (exists)
        {
            throw new BusinessException("Bu e-posta adresi zaten kullanılıyor");
        }

        // Entity oluştur ve kaydet
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Role = request.Role
        };

        await _unitOfWork.Users.CreateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User created: {Email}", user.Email);

        return user.ToDto();
    }
}
```

### 3.3 Exception Fırlatma Kuralları

```csharp
// ✅ Doğru: Spesifik exception tipleri kullan
public async Task<ProductDetailDto> GetProductDetailAsync(int id, CancellationToken ct)
{
    var product = await _nebimRepository.GetProductByIdAsync(id, ct);
    
    if (product is null)
    {
        throw new NotFoundException($"Ürün bulunamadı: {id}");
    }

    return MapToDetailDto(product);
}

// ✅ Doğru: İş kuralı ihlali için BusinessException
public async Task DeleteUserAsync(Guid id, CancellationToken ct)
{
    var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Kullanıcı bulunamadı: {id}");

    if (user.Role == UserRole.Admin)
    {
        var adminCount = await _unitOfWork.Users.CountAdminsAsync(ct);
        if (adminCount <= 1)
        {
            throw new BusinessException("Son admin kullanıcısı silinemez");
        }
    }

    await _unitOfWork.Users.DeleteAsync(id, ct);
    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 4. Transaction Yönetimi

### 4.1 Basit İşlemler (Tek SaveChanges)

```csharp
// ✅ Basit CRUD - Unit of Work yeterli
public async Task UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
{
    var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Kullanıcı bulunamadı: {id}");

    user.FullName = request.FullName;
    user.Role = request.Role;

    await _unitOfWork.Users.UpdateAsync(user, ct);
    await _unitOfWork.SaveChangesAsync(ct);  // Tek SaveChanges = otomatik transaction
}
```

### 4.2 Karmaşık İşlemler (Explicit Transaction)

```csharp
// ✅ Birden fazla aggregate güncelleniyorsa explicit transaction kullan
public async Task TransferChatSessionAsync(
    Guid sessionId, 
    Guid newUserId, 
    CancellationToken ct)
{
    await _unitOfWork.BeginTransactionAsync(ct);
    
    try
    {
        // 1. Session'ı bul
        var session = await _unitOfWork.Chats.GetSessionByIdAsync(sessionId, ct)
            ?? throw new NotFoundException($"Session bulunamadı: {sessionId}");

        // 2. Yeni kullanıcıyı doğrula
        var newUser = await _unitOfWork.Users.GetByIdAsync(newUserId, ct)
            ?? throw new NotFoundException($"Kullanıcı bulunamadı: {newUserId}");

        // 3. Eski kullanıcıya bildirim ekle
        await _unitOfWork.Notifications.CreateAsync(new Notification
        {
            UserId = session.UserId,
            Message = $"'{session.Title}' başlıklı sohbet {newUser.FullName}'e aktarıldı"
        }, ct);

        // 4. Session'ı aktar
        session.UserId = newUserId;
        await _unitOfWork.Chats.UpdateSessionAsync(session, ct);

        // 5. Yeni kullanıcıya bildirim ekle
        await _unitOfWork.Notifications.CreateAsync(new Notification
        {
            UserId = newUserId,
            Message = $"'{session.Title}' başlıklı sohbet size aktarıldı"
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);

        _logger.LogInformation(
            "Chat session {SessionId} transferred from {OldUser} to {NewUser}",
            sessionId, session.UserId, newUserId);
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        _logger.LogError(ex, "Failed to transfer chat session {SessionId}", sessionId);
        throw;
    }
}
```

### 4.3 Transaction Kuralları

```csharp
// ✅ KURALLAR:
// 1. Tek aggregate güncellemesi = SaveChanges yeterli
// 2. Birden fazla aggregate = Explicit transaction
// 3. Cross-service işlem = Saga pattern veya eventual consistency düşün
// 4. Nebim DB'ye ASLA yazma yapılmaz!

// ❌ YANLIŞ: Nebim ve App DB'yi aynı transaction'da kullanma
public async Task SyncProductAsync(int nebimProductId)
{
    await _unitOfWork.BeginTransactionAsync();  // Bu sadece AppDB için!
    
    var product = await _nebimRepository.GetProductAsync(nebimProductId);  // Farklı DB!
    // ... bu pattern doğru değil
}

// ✅ DOĞRU: Ayrı ayrı işle
public async Task SyncProductAsync(int nebimProductId, CancellationToken ct)
{
    // 1. Nebim'den oku (transaction dışında)
    var nebimProduct = await _nebimRepository.GetProductByIdAsync(nebimProductId, ct);
    
    // 2. App DB'de işle
    var localCache = await _unitOfWork.ProductCache.GetByNebimIdAsync(nebimProductId, ct);
    if (localCache is null)
    {
        await _unitOfWork.ProductCache.CreateAsync(new ProductCache 
        { 
            NebimProductId = nebimProductId,
            LastSyncedAt = DateTime.UtcNow
        }, ct);
    }
    else
    {
        localCache.LastSyncedAt = DateTime.UtcNow;
        await _unitOfWork.ProductCache.UpdateAsync(localCache, ct);
    }
    
    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 5. Dependency Injection

### 5.1 Constructor Injection (Tercih Edilen)

```csharp
// ✅ Doğru: Constructor injection
public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INebimRepository _nebimRepository;
    private readonly IChatOrchestrator _chatOrchestrator;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IUnitOfWork unitOfWork,
        INebimRepository nebimRepository,
        IChatOrchestrator chatOrchestrator,
        ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _nebimRepository = nebimRepository;
        _chatOrchestrator = chatOrchestrator;
        _logger = logger;
    }
}

// ❌ Yanlış: Service Locator pattern
public class ChatService : IChatService
{
    private readonly IServiceProvider _serviceProvider;
    
    public async Task DoSomething()
    {
        // ❌ YANLIŞ! Anti-pattern
        var repository = _serviceProvider.GetRequiredService<IUserRepository>();
    }
}
```

### 5.2 Service Kayıt

```csharp
// Program.cs veya ServiceCollectionExtensions.cs

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        // AI
        services.AddScoped<IChatOrchestrator, ChatOrchestrator>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        return services;
    }
}
```

### 5.3 Lifetime Kuralları

| Lifetime | Kullanım | Örnek |
|----------|----------|-------|
| **Scoped** | Request başına bir instance | Service, Repository, DbContext |
| **Singleton** | Uygulama boyunca tek instance | Configuration, Cache |
| **Transient** | Her resolve'da yeni instance | Validator, Helper |

```csharp
// ✅ Doğru lifetime seçimleri
services.AddScoped<IChatService, ChatService>();         // Request-scoped
services.AddSingleton<ICacheService, MemoryCacheService>();  // Singleton cache
services.AddTransient<IPasswordHelper, PasswordHelper>();    // Stateless helper
```

---

## 6. Semantic Kernel (AI) Entegrasyonu

### 6.1 ChatOrchestrator

```csharp
namespace BLL.AI;

public interface IChatOrchestrator
{
    Task<ChatResponse> ProcessMessageAsync(
        Guid sessionId, 
        string userMessage, 
        CancellationToken ct = default);
}

public class ChatOrchestrator : IChatOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatOrchestrator> _logger;

    public ChatOrchestrator(
        Kernel kernel,
        IUnitOfWork unitOfWork,
        ILogger<ChatOrchestrator> logger)
    {
        _kernel = kernel;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessMessageAsync(
        Guid sessionId, 
        string userMessage, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Processing message for session {SessionId}", sessionId);

        // 1. Kullanıcı mesajını kaydet
        var userChatMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = MessageRole.User,
            Content = userMessage,
            Timestamp = DateTime.UtcNow
        };
        await _unitOfWork.Chats.AddMessageAsync(userChatMessage, ct);

        // 2. Geçmiş mesajları al
        var history = await _unitOfWork.Chats.GetSessionMessagesAsync(sessionId, ct);
        
        // 3. Chat history oluştur
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(await GetSystemPromptAsync());
        
        foreach (var msg in history.OrderBy(m => m.Timestamp))
        {
            if (msg.Role == MessageRole.User)
                chatHistory.AddUserMessage(msg.Content);
            else if (msg.Role == MessageRole.Assistant)
                chatHistory.AddAssistantMessage(msg.Content);
        }
        chatHistory.AddUserMessage(userMessage);

        // 4. AI yanıtı al
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings: new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 2048,
                Temperature = 0.7
            },
            kernel: _kernel,
            cancellationToken: ct);

        // 5. AI yanıtını kaydet
        var assistantMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = MessageRole.Assistant,
            Content = response.Content ?? string.Empty,
            Timestamp = DateTime.UtcNow
        };
        await _unitOfWork.Chats.AddMessageAsync(assistantMessage, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AI response generated for session {SessionId}", sessionId);

        return new ChatResponse
        {
            SessionId = sessionId,
            Message = response.Content ?? string.Empty,
            Role = MessageRole.Assistant,
            Timestamp = assistantMessage.Timestamp
        };
    }

    private async Task<string> GetSystemPromptAsync()
    {
        // Dosyadan veya cache'den sistem prompt'unu al
        return """
            Sen Nebim V3 ERP sistemi için bir AI asistanısın.
            Kullanıcılara satış, stok ve ürün bilgileri konusunda yardımcı oluyorsun.
            Türkçe yanıt ver. Kısa ve öz ol.
            Veritabanı sorguları için sana verilen fonksiyonları kullan.
            """;
    }
}
```

### 6.2 Semantic Kernel Plugin

```csharp
namespace BLL.AI.Plugins;

public class NebimQueryPlugin
{
    private readonly INebimRepository _nebimRepository;
    private readonly ILogger<NebimQueryPlugin> _logger;

    public NebimQueryPlugin(INebimRepository nebimRepository, ILogger<NebimQueryPlugin> logger)
    {
        _nebimRepository = nebimRepository;
        _logger = logger;
    }

    [KernelFunction("GetTodaySalesSummary")]
    [Description("Bugünkü satış özetini getirir: toplam ciro, satış adedi, iade tutarı")]
    public async Task<string> GetTodaySalesSummaryAsync()
    {
        _logger.LogDebug("AI calling GetTodaySalesSummary");
        
        var sales = await _nebimRepository.GetTodaySalesAsync();
        
        return $"""
            📊 Bugünkü Satış Özeti ({DateTime.Today:dd MMMM yyyy})
            - Toplam Ciro: {sales.TotalRevenue:C}
            - İşlem Sayısı: {sales.TransactionCount}
            - İade Tutarı: {sales.ReturnAmount:C}
            - Net Satış: {sales.NetSales:C}
            """;
    }

    [KernelFunction("GetLowStockProducts")]
    [Description("Kritik stok seviyesindeki ürünleri listeler")]
    [return: Description("Stok durumu kritik olan ürünlerin listesi")]
    public async Task<string> GetLowStockProductsAsync(
        [Description("Kaç ürün listelensin (varsayılan: 10)")] int limit = 10)
    {
        _logger.LogDebug("AI calling GetLowStockProducts with limit {Limit}", limit);
        
        var products = await _nebimRepository.GetLowStockAlertsAsync(threshold: 20);
        var topProducts = products.Take(limit);

        if (!topProducts.Any())
        {
            return "✅ Şu anda kritik stok seviyesinde ürün bulunmuyor.";
        }

        var result = new StringBuilder();
        result.AppendLine("⚠️ Kritik Stok Seviyesindeki Ürünler:");
        result.AppendLine();
        
        foreach (var p in topProducts)
        {
            result.AppendLine($"- {p.ProductName} ({p.ProductCode})");
            result.AppendLine($"  Stok: {p.CurrentStock} / Min: {p.MinStock}");
        }

        return result.ToString();
    }

    [KernelFunction("SearchProducts")]
    [Description("Ürün adı veya koduna göre ürün arar")]
    public async Task<string> SearchProductsAsync(
        [Description("Aranacak ürün adı veya kodu")] string searchTerm,
        [Description("Maksimum sonuç sayısı")] int limit = 5)
    {
        _logger.LogDebug("AI calling SearchProducts: {SearchTerm}", searchTerm);
        
        var filter = new StockFilterRequest { SearchTerm = searchTerm, PageSize = limit };
        var result = await _nebimRepository.GetProductsAsync(filter);

        if (!result.Items.Any())
        {
            return $"'{searchTerm}' için ürün bulunamadı.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🔍 '{searchTerm}' için {result.TotalCount} sonuç bulundu:");
        sb.AppendLine();

        foreach (var p in result.Items)
        {
            sb.AppendLine($"- {p.Name} ({p.Code})");
            sb.AppendLine($"  Fiyat: {p.Price:C} | Stok: {p.TotalStock}");
        }

        return sb.ToString();
    }
}
```

### 6.3 Kernel Konfigürasyonu

```csharp
// Program.cs
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Google Gemini ekle
    kernelBuilder.AddGoogleAIGeminiChatCompletion(
        modelId: config["AI:Model"] ?? "gemini-1.5-flash",
        apiKey: config["AI:ApiKey"]!);
    
    // Plugin'leri ekle
    kernelBuilder.Plugins.AddFromObject(
        sp.GetRequiredService<NebimQueryPlugin>(), 
        "NebimQuery");
    
    return kernelBuilder.Build();
});

// Plugin'i ayrıca kaydet (DI için)
builder.Services.AddScoped<NebimQueryPlugin>();
```

---

## 7. Caching Stratejisi

### 7.1 Memory Cache Kullanımı

```csharp
public class DashboardService : IDashboardService
{
    private readonly IMemoryCache _cache;
    private readonly INebimRepository _nebimRepository;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
    {
        const string cacheKey = "categories_all";

        // ✅ Cache'den al veya yükle
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            entry.Priority = CacheItemPriority.Normal;
            
            _logger.LogDebug("Cache miss for {CacheKey}, loading from database", cacheKey);
            return await _nebimRepository.GetCategoriesAsync(ct);
        }) ?? [];
    }
}
```

### 7.2 Cache Invalidation

```csharp
// ✅ Doğru: İlgili cache'leri temizle
public async Task InvalidateDashboardCacheAsync()
{
    _cache.Remove("dashboard_kpi");
    _cache.Remove("dashboard_weekly_sales");
    _cache.Remove("categories_all");
    
    _logger.LogInformation("Dashboard cache invalidated");
}
```

---

## 8. Mapping (DTO Dönüşümleri)

### 8.1 Extension Method ile Mapping

```csharp
namespace BLL.Mappings;

public static class MappingExtensions
{
    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            Role = entity.Role,
            AvatarUrl = entity.AvatarUrl,
            CreatedAt = entity.CreatedAt
        };
    }

    public static IEnumerable<UserDto> ToDtoList(this IEnumerable<User> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static ChatSessionDto ToDto(this ChatSession entity, int messageCount = 0)
    {
        return new ChatSessionDto
        {
            Id = entity.Id,
            Title = entity.Title,
            CreatedAt = entity.CreatedAt,
            MessageCount = messageCount
        };
    }
}

// Kullanım:
var userDto = user.ToDto();
var userDtos = users.ToDtoList();
```

### 8.2 Mapping Kuralları

```csharp
// ✅ Doğru: Mapping BLL'de yapılır
public async Task<UserDto> GetUserAsync(Guid id, CancellationToken ct)
{
    var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Kullanıcı bulunamadı: {id}");
    
    return user.ToDto();  // Mapping burada
}

// ❌ Yanlış: Controller'da mapping
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id)
{
    var user = await _userRepository.GetByIdAsync(id);
    return new UserDto { ... };  // ❌ Controller'da mapping yapılmamalı
}

// ❌ Yanlış: Repository'de mapping
public async Task<UserDto> GetByIdAsync(Guid id)
{
    var user = await _context.Users.FindAsync(id);
    return new UserDto { ... };  // ❌ Repository DTO döndürmemeli
}
```

---

## 📝 Kontrol Listesi

BLL kodu yazarken şunları kontrol et:

- [ ] Service interface `I` prefix'i ile tanımlandı mı?
- [ ] Service'te doğrudan veritabanı sorgusu yok mu?
- [ ] İş mantığı sadece BLL'de mi?
- [ ] Validation FluentValidation ile mi yapılıyor?
- [ ] Exception'lar doğru tipte mi? (`NotFoundException`, `BusinessException`)
- [ ] Constructor injection kullanılıyor mu?
- [ ] Bağımsız işlemler paralel mi çalıştırılıyor?
- [ ] Transaction gerekli yerlerde kullanılıyor mu?
- [ ] CancellationToken parametresi var mı?
- [ ] Mapping extension method'ları kullanılıyor mu?
- [ ] Loglama yapılıyor mu?

---

*Son Güncelleme: 26 Aralık 2025*
