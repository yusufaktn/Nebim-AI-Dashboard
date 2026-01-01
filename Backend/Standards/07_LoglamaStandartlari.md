# 📝 Loglama Standartları

> Bu doküman, Serilog kullanımı, log seviyeleri, structured logging, correlation ID ve hassas veri maskeleme kurallarını tanımlar.

---

## 📌 İçindekiler

1. [Serilog Konfigürasyonu](#1-serilog-konfigürasyonu)
2. [Log Seviyeleri](#2-log-seviyeleri)
3. [Structured Logging](#3-structured-logging)
4. [Correlation ID](#4-correlation-id)
5. [Ne Loglanmalı?](#5-ne-loglanmalı)
6. [Hassas Veri Maskeleme](#6-hassas-veri-maskeleme)
7. [Performance Logging](#7-performance-logging)
8. [Log Çıktı Hedefleri](#8-log-çıktı-hedefleri)

---

## 1. Serilog Konfigürasyonu

### 1.1 NuGet Paketleri

```xml
<!-- Api.csproj -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />
```

### 1.2 Program.cs Konfigürasyonu

```csharp
using Serilog;
using Serilog.Events;

// Bootstrap logger (uygulama başlamadan önce hatalar için)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Nebim Dashboard API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog konfigürasyonu
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "NebimDashboard"));

    // ... diğer servisler

    var app = builder.Build();

    // Request logging middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = EnrichDiagnosticContext;
    });

    // ... diğer middleware'ler

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Enrichment helper
static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
{
    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
    diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
    diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
    
    if (httpContext.User.Identity?.IsAuthenticated == true)
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
        diagnosticContext.Set("UserEmail", httpContext.User.FindFirst("email")?.Value);
    }
}
```

### 1.3 appsettings.json Konfigürasyonu

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/nebim-dashboard-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

---

## 2. Log Seviyeleri

### 2.1 Seviye Tanımları

| Seviye | Kullanım | Örnek |
|--------|----------|-------|
| **Verbose** | En detaylı, debug için | Değişken değerleri, loop iterasyonları |
| **Debug** | Geliştirme detayları | Method giriş/çıkış, SQL sorguları |
| **Information** | Normal akış | Request başladı, kullanıcı giriş yaptı |
| **Warning** | Potansiyel sorun | Retry yapıldı, deprecated API kullanıldı |
| **Error** | Hata, ama uygulama çalışıyor | Exception yakalandı, işlem başarısız |
| **Fatal** | Kritik hata, uygulama durmalı | Database bağlantısı yok, config eksik |

### 2.2 Seviye Kullanım Örnekleri

```csharp
public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken ct)
    {
        // ✅ Debug: Method başlangıcı
        _logger.LogDebug("Getting product by ID: {ProductId}", id);

        var product = await _nebimRepository.GetProductByIdAsync(id, ct);

        if (product is null)
        {
            // ✅ Warning: Kayıt bulunamadı (hata değil ama dikkat edilmeli)
            _logger.LogWarning("Product not found: {ProductId}", id);
            throw new NotFoundException("Ürün", id);
        }

        // ✅ Debug: Başarılı sonuç
        _logger.LogDebug("Product retrieved: {ProductId}, Name: {ProductName}", id, product.Name);

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken ct)
    {
        // ✅ Information: Önemli iş olayı
        _logger.LogInformation("Creating new product: {ProductCode}", request.Code);

        try
        {
            var product = await _repository.CreateAsync(MapToEntity(request), ct);
            
            // ✅ Information: Başarılı tamamlama
            _logger.LogInformation(
                "Product created successfully: {ProductId}, Code: {ProductCode}",
                product.Id,
                product.Code);

            return MapToDto(product);
        }
        catch (Exception ex)
        {
            // ✅ Error: Hata durumu
            _logger.LogError(
                ex,
                "Failed to create product: {ProductCode}",
                request.Code);
            throw;
        }
    }
}
```

### 2.3 Seviye Seçim Kuralları

```csharp
// ✅ Information: Kullanıcı aksiyonları
_logger.LogInformation("User {UserId} logged in", userId);
_logger.LogInformation("Order {OrderId} created by user {UserId}", orderId, userId);
_logger.LogInformation("Product {ProductId} updated", productId);

// ✅ Warning: Dikkat edilmesi gereken durumlar
_logger.LogWarning("Low stock alert for product {ProductId}: {Stock} remaining", productId, stock);
_logger.LogWarning("Retry attempt {Attempt} for external API call", attempt);
_logger.LogWarning("Cache miss for key {CacheKey}", cacheKey);

// ✅ Error: Yakalanan hatalar
_logger.LogError(ex, "Database connection failed");
_logger.LogError(ex, "Failed to process order {OrderId}", orderId);

// ✅ Fatal: Uygulama durmalı
_logger.LogCritical(ex, "Unable to start application: configuration missing");
_logger.LogCritical("Database migration failed, shutting down");
```

---

## 3. Structured Logging

### 3.1 Temel Kurallar

```csharp
// ✅ DOĞRU: Structured logging - template + parametreler
_logger.LogInformation(
    "User {UserId} created order {OrderId} with {ItemCount} items for {TotalAmount:C}",
    userId,
    orderId,
    items.Count,
    totalAmount);

// ❌ YANLIŞ: String interpolation (aranabilir değil!)
_logger.LogInformation($"User {userId} created order {orderId}");

// ❌ YANLIŞ: String concatenation
_logger.LogInformation("User " + userId + " created order " + orderId);
```

### 3.2 Property İsimlendirme

```csharp
// ✅ PascalCase kullan
_logger.LogInformation("Processing {OrderId} for {CustomerId}", orderId, customerId);

// ✅ Anlamlı isimler
_logger.LogInformation(
    "Stock updated: Product {ProductId}, Previous: {PreviousStock}, New: {NewStock}",
    productId,
    previousStock,
    newStock);

// ✅ @ operatörü ile obje destructuring
_logger.LogInformation("Order created: {@Order}", order);

// ✅ $ operatörü ile ToString()
_logger.LogInformation("User type: {$UserType}", userType);
```

### 3.3 Obje Loglama

```csharp
// ✅ Destructuring ile tüm property'leri logla
_logger.LogInformation("New order received: {@OrderRequest}", request);

// Çıktı:
// New order received: {"CustomerId": "123", "Items": [...], "TotalAmount": 1500.00}

// ✅ Özet bilgi için seçili alanlar
_logger.LogInformation(
    "Order summary: OrderId={OrderId}, Customer={CustomerId}, Total={Total:C}",
    order.Id,
    order.CustomerId,
    order.TotalAmount);
```

---

## 4. Correlation ID

### 4.1 Correlation ID Middleware

```csharp
namespace Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Header'dan al veya yeni oluştur
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Response header'a ekle
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // LogContext'e ekle (tüm loglar bu ID'yi içerecek)
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
```

### 4.2 Service'lerde Correlation ID Kullanımı

```csharp
// Tüm loglar otomatik olarak CorrelationId içerir
// Çıktı örneği:
// [14:30:45 INF] [abc123-def456] Processing order 12345
// [14:30:45 INF] [abc123-def456] Validating order items
// [14:30:46 INF] [abc123-def456] Order created successfully
```

### 4.3 Cross-Service Correlation

```csharp
// Başka bir servisi çağırırken Correlation ID'yi ilet
public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<T> GetAsync<T>(string url)
    {
        var correlationId = _httpContextAccessor.HttpContext?
            .Request.Headers["X-Correlation-Id"].FirstOrDefault();

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-Id", correlationId);
        }

        var response = await _httpClient.SendAsync(request);
        // ...
    }
}
```

---

## 5. Ne Loglanmalı?

### 5.1 Loglanması Gereken Olaylar

| Kategori | Örnekler | Seviye |
|----------|----------|--------|
| **Kimlik Doğrulama** | Giriş, çıkış, başarısız deneme | Information/Warning |
| **Yetkilendirme** | Yetkisiz erişim denemesi | Warning |
| **CRUD İşlemleri** | Kayıt oluşturma, güncelleme, silme | Information |
| **İş Kuralları** | Önemli karar noktaları | Information |
| **Hatalar** | Tüm exception'lar | Error |
| **Performans** | Yavaş sorgular, timeout'lar | Warning |
| **Entegrasyonlar** | API çağrıları, response'lar | Debug/Information |

### 5.2 Loglama Örnekleri

```csharp
// ✅ Authentication olayları
_logger.LogInformation("User {Email} logged in from {IpAddress}", email, ipAddress);
_logger.LogWarning("Failed login attempt for {Email} from {IpAddress}", email, ipAddress);
_logger.LogInformation("User {UserId} logged out", userId);

// ✅ Authorization olayları
_logger.LogWarning(
    "Unauthorized access attempt: User {UserId} tried to access {Resource}",
    userId,
    resource);

// ✅ CRUD olayları
_logger.LogInformation("Product created: {ProductId} by user {UserId}", productId, userId);
_logger.LogInformation("Order {OrderId} updated: Status changed to {NewStatus}", orderId, newStatus);
_logger.LogInformation("User {TargetUserId} deleted by admin {AdminId}", targetUserId, adminId);

// ✅ İş kuralları
_logger.LogInformation(
    "Discount applied: Order {OrderId}, Original: {OriginalAmount:C}, Discounted: {FinalAmount:C}",
    orderId,
    originalAmount,
    finalAmount);

// ✅ Entegrasyonlar
_logger.LogDebug("Calling Nebim API: {Endpoint}", endpoint);
_logger.LogDebug("Nebim API response: {StatusCode} in {ElapsedMs}ms", statusCode, elapsed);
```

### 5.3 Loglanmaması Gerekenler

```csharp
// ❌ Hassas veriler
_logger.LogInformation("User logged in with password {Password}");  // ASLA!
_logger.LogInformation("Credit card: {CardNumber}");  // ASLA!

// ❌ Kişisel veriler (KVKK/GDPR)
_logger.LogInformation("Customer details: {@Customer}");  // Maskele!

// ❌ Çok fazla detay (spam)
foreach (var item in items)
{
    _logger.LogDebug("Processing item {ItemId}", item.Id);  // Çok fazla log!
}

// ✅ Bunun yerine özet logla
_logger.LogDebug("Processing {ItemCount} items", items.Count);
```

---

## 6. Hassas Veri Maskeleme

### 6.1 Destructuring Policy

```csharp
// Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Destructure.ByTransforming<User>(u => new
    {
        u.Id,
        u.Email,  // Email gösterilsin
        Password = "***MASKED***",  // Şifre maskelensin
        u.Role
    })
    .Destructure.ByTransforming<CreateUserRequest>(r => new
    {
        r.FullName,
        r.Email,
        Password = "***MASKED***"
    }));
```

### 6.2 Custom Enricher ile Maskeleme

```csharp
public class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "PasswordHash",
        "Token",
        "AccessToken",
        "RefreshToken",
        "ApiKey",
        "Secret",
        "CreditCard",
        "CardNumber",
        "Cvv"
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var propertiesToUpdate = new List<LogEventProperty>();

        foreach (var property in logEvent.Properties)
        {
            if (SensitiveProperties.Contains(property.Key))
            {
                propertiesToUpdate.Add(
                    propertyFactory.CreateProperty(property.Key, "***MASKED***"));
            }
        }

        foreach (var property in propertiesToUpdate)
        {
            logEvent.AddOrUpdateProperty(property);
        }
    }
}

// Kayıt
configuration.Enrich.With<SensitiveDataMaskingEnricher>();
```

### 6.3 Email Maskeleme Helper

```csharp
public static class LoggingHelpers
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return "***@" + email[(atIndex + 1)..];
        
        return email[0] + "***" + email[(atIndex - 1)..];
        // "john.doe@example.com" -> "j***e@example.com"
    }

    public static string MaskCreditCard(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            return "****";
        
        return "****-****-****-" + cardNumber[^4..];
        // "1234567890123456" -> "****-****-****-3456"
    }
}

// Kullanım
_logger.LogInformation("Payment processed for card {CardNumber}", 
    LoggingHelpers.MaskCreditCard(cardNumber));
```

---

## 7. Performance Logging

### 7.1 Execution Time Logging

```csharp
public class StockService : IStockService
{
    private readonly ILogger<StockService> _logger;
    private readonly Stopwatch _stopwatch = new();

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        StockFilterRequest filter, 
        CancellationToken ct)
    {
        _stopwatch.Restart();
        
        var result = await _nebimRepository.GetProductsAsync(filter, ct);
        
        _stopwatch.Stop();
        
        // ✅ Performance log
        if (_stopwatch.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning(
                "Slow query detected: GetProducts took {ElapsedMs}ms with filter {@Filter}",
                _stopwatch.ElapsedMilliseconds,
                filter);
        }
        else
        {
            _logger.LogDebug(
                "GetProducts completed in {ElapsedMs}ms",
                _stopwatch.ElapsedMilliseconds);
        }

        return result;
    }
}
```

### 7.2 Scope ile Timing

```csharp
public async Task<DashboardResponse> GetDashboardAsync(CancellationToken ct)
{
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["Operation"] = "GetDashboard",
        ["StartTime"] = DateTime.UtcNow
    }))
    {
        var sw = Stopwatch.StartNew();
        
        _logger.LogInformation("Dashboard data fetch started");
        
        var result = await FetchDashboardDataAsync(ct);
        
        sw.Stop();
        _logger.LogInformation(
            "Dashboard data fetch completed in {ElapsedMs}ms",
            sw.ElapsedMilliseconds);
        
        return result;
    }
}
```

### 7.3 Database Query Logging

```csharp
// EF Core query logging (appsettings.json)
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"  // SQL logla
      }
    }
  }
}

// Veya sadece yavaş sorgular için
services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString)
           .LogTo(
               message => Log.Warning("Slow query: {Query}", message),
               new[] { DbLoggerCategory.Database.Command.Name },
               LogLevel.Warning,
               DbContextLoggerOptions.SingleLine);
});
```

---

## 8. Log Çıktı Hedefleri

### 8.1 Development Ortamı

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      }
    ]
  }
}
```

### 8.2 Production Ortamı

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/nebim-dashboard/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600,
          "rollOnFileSizeLimit": true
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "https://seq.example.com",
          "apiKey": "YOUR_API_KEY"
        }
      }
    ]
  }
}
```

### 8.3 Seq ile Merkezi Loglama

```csharp
// Docker ile Seq kurulumu
// docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq

// Seq'e log gönderme
.WriteTo.Seq("http://localhost:5341")

// Seq avantajları:
// - Structured log arama
// - Dashboard ve alerting
// - Correlation ID ile trace
// - Retention policy
```

---

## 📝 Kontrol Listesi

Loglama yazarken şunları kontrol et:

- [ ] Structured logging kullanılıyor mu? (template + parametreler)
- [ ] Log seviyesi doğru mu? (Debug/Info/Warning/Error)
- [ ] Hassas veriler maskeleniyor mu? (password, token, card)
- [ ] Correlation ID mevcut mu?
- [ ] CRUD işlemleri loglanıyor mu?
- [ ] Authentication olayları loglanıyor mu?
- [ ] Error loglarında exception dahil mi?
- [ ] Performance sorunları (yavaş query) uyarı veriyor mu?
- [ ] Log seviyesi ortama göre ayarlanmış mı?
- [ ] Property isimleri PascalCase mi?

---

*Son Güncelleme: 26 Aralık 2025*
