# 🌐 API (Web API) Standartları

> Bu doküman, ASP.NET Core Web API katmanındaki Controller tasarımı, response format, HTTP status kodları ve Swagger dokümantasyonunu tanımlar.

---

## 📌 İçindekiler

1. [Klasör Yapısı](#1-klasör-yapısı)
2. [Controller Tasarım Kuralları](#2-controller-tasarım-kuralları)
3. [ApiResponse Wrapper](#3-apiresponse-wrapper)
4. [HTTP Status Kodları](#4-http-status-kodları)
5. [Route Naming Conventions](#5-route-naming-conventions)
6. [Request/Response Handling](#6-requestresponse-handling)
7. [Swagger/OpenAPI Dokümantasyonu](#7-swaggeropenapi-dokümantasyonu)
8. [CORS ve Güvenlik](#8-cors-ve-güvenlik)

---

## 1. Klasör Yapısı

```
Api/
├── Controllers/
│   ├── DashboardController.cs
│   ├── StockController.cs
│   ├── ChatController.cs
│   ├── AuthController.cs
│   └── UsersController.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── CorrelationIdMiddleware.cs
│
├── Filters/
│   ├── ValidationFilter.cs
│   └── ApiExceptionFilter.cs
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
│
├── Common/
│   ├── ApiResponse.cs
│   └── ApiErrorResponse.cs
│
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## 2. Controller Tasarım Kuralları

### 2.1 Temel Controller Yapısı

```csharp
namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard ana verilerini getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard(
        CancellationToken cancellationToken)
    {
        var data = await _dashboardService.GetDashboardDataAsync(cancellationToken);
        return Ok(ApiResponse<DashboardResponse>.Success(data));
    }

    /// <summary>
    /// Haftalık satış verilerini getirir
    /// </summary>
    [HttpGet("weekly-sales")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklySalesDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklySalesDto>>>> GetWeeklySales(
        [FromQuery] int weeks = 1,
        CancellationToken cancellationToken = default)
    {
        var data = await _dashboardService.GetWeeklySalesAsync(weeks, cancellationToken);
        return Ok(ApiResponse<IEnumerable<WeeklySalesDto>>.Success(data));
    }
}
```

### 2.2 Controller'da İş Mantığı OLMAZ!

```csharp
// ❌ YANLIŞ: Controller'da iş mantığı
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(int id)
{
    var product = await _repository.GetByIdAsync(id);
    
    // ❌ YANLIŞ! Bu mantık BLL'de olmalı
    if (product.Stock < product.MinStock)
    {
        product.Status = StockStatus.LowStock;
        await _notificationService.SendLowStockAlert(product);
    }
    
    // ❌ YANLIŞ! Mapping BLL'de olmalı
    return new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        // ...
    };
}

// ✅ DOĞRU: Controller sadece HTTP işlemi yapar
[HttpGet("{id}")]
[ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(
    int id, 
    CancellationToken cancellationToken)
{
    var product = await _productService.GetProductByIdAsync(id, cancellationToken);
    return Ok(ApiResponse<ProductDto>.Success(product));
}
```

### 2.3 Controller Sorumlulukları

| ✅ Controller'ın Görevi | ❌ Controller'da Olmaması Gerekenler |
|------------------------|-------------------------------------|
| HTTP request/response yönetimi | İş mantığı |
| Route tanımlama | Veritabanı sorguları |
| Model binding | DTO mapping |
| Authorization attribute'ları | Validation mantığı |
| Status code belirleme | Exception handling (middleware'de) |
| Swagger dokümantasyonu | Loglama detayları |

### 2.4 Slim Controller Örneği

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll(CancellationToken ct)
        => Ok(ApiResponse<IEnumerable<UserDto>>.Success(
            await _userService.GetAllUsersAsync(ct)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<UserDto>.Success(
            await _userService.GetUserByIdAsync(id, ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(
        CreateUserRequest request, 
        CancellationToken ct)
    {
        var user = await _userService.CreateUserAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, 
            ApiResponse<UserDto>.Success(user, "Kullanıcı başarıyla oluşturuldu"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(
        Guid id, 
        UpdateUserRequest request, 
        CancellationToken ct)
    {
        var user = await _userService.UpdateUserAsync(id, request, ct);
        return Ok(ApiResponse<UserDto>.Success(user, "Kullanıcı güncellendi"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _userService.DeleteUserAsync(id, ct);
        return Ok(ApiResponse<object>.Success(null, "Kullanıcı silindi"));
    }
}
```

---

## 3. ApiResponse Wrapper

### 3.1 ApiResponse Sınıfı

```csharp
namespace Api.Common;

/// <summary>
/// Standart API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message
        };
    }
}

/// <summary>
/// Hata durumlarında kullanılan response
/// </summary>
public class ApiErrorResponse
{
    public bool IsSuccess { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiErrorResponse Create(
        string message, 
        string? errorCode = null,
        IDictionary<string, string[]>? errors = null)
    {
        return new ApiErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            Errors = errors,
            TraceId = Activity.Current?.Id
        };
    }

    public static ApiErrorResponse ValidationError(IDictionary<string, string[]> errors)
    {
        return new ApiErrorResponse
        {
            Message = "Doğrulama hatası",
            ErrorCode = "VALIDATION_ERROR",
            Errors = errors,
            TraceId = Activity.Current?.Id
        };
    }
}
```

### 3.2 Sayfalı Response

```csharp
/// <summary>
/// Sayfalı veriler için response wrapper
/// </summary>
public class PagedApiResponse<T>
{
    public bool IsSuccess { get; set; } = true;
    public List<T> Items { get; set; } = [];
    public PaginationMeta Pagination { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static PagedApiResponse<T> Success(PagedResult<T> pagedResult)
    {
        return new PagedApiResponse<T>
        {
            Items = pagedResult.Items,
            Pagination = new PaginationMeta
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            }
        };
    }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

### 3.3 Kullanım Örnekleri

```csharp
// ✅ Basit response
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id, CancellationToken ct)
{
    var product = await _productService.GetProductByIdAsync(id, ct);
    return Ok(ApiResponse<ProductDto>.Success(product));
}

// ✅ Mesajlı response
[HttpPost]
public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
    CreateUserRequest request, 
    CancellationToken ct)
{
    var user = await _userService.CreateUserAsync(request, ct);
    return CreatedAtAction(
        nameof(GetById), 
        new { id = user.Id },
        ApiResponse<UserDto>.Success(user, "Kullanıcı başarıyla oluşturuldu"));
}

// ✅ Sayfalı response
[HttpGet]
public async Task<ActionResult<PagedApiResponse<ProductListItemDto>>> GetProducts(
    [FromQuery] StockFilterRequest filter,
    CancellationToken ct)
{
    var pagedResult = await _stockService.GetProductsAsync(filter, ct);
    return Ok(PagedApiResponse<ProductListItemDto>.Success(pagedResult));
}
```

---

## 4. HTTP Status Kodları

### 4.1 Başarılı İşlemler (2xx)

| Status Code | Kullanım | Örnek |
|-------------|----------|-------|
| **200 OK** | Başarılı GET, PUT, PATCH | Veri getirme, güncelleme |
| **201 Created** | Başarılı POST (yeni kayıt) | Kullanıcı oluşturma |
| **204 No Content** | Başarılı DELETE, içerik yok | Silme işlemi |

```csharp
// 200 OK - Veri getirme
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id, CancellationToken ct)
{
    var product = await _service.GetByIdAsync(id, ct);
    return Ok(ApiResponse<ProductDto>.Success(product));
}

// 201 Created - Yeni kayıt
[HttpPost]
public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequest request, CancellationToken ct)
{
    var user = await _service.CreateAsync(request, ct);
    return CreatedAtAction(nameof(Get), new { id = user.Id }, 
        ApiResponse<UserDto>.Success(user));
}

// 204 No Content - Silme
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    await _service.DeleteAsync(id, ct);
    return NoContent();
}
```

### 4.2 İstemci Hataları (4xx)

| Status Code | Kullanım | Exception Tipi |
|-------------|----------|----------------|
| **400 Bad Request** | Geçersiz istek | `ValidationException` |
| **401 Unauthorized** | Kimlik doğrulama gerekli | `UnauthorizedException` |
| **403 Forbidden** | Yetki yetersiz | `ForbiddenException` |
| **404 Not Found** | Kayıt bulunamadı | `NotFoundException` |
| **409 Conflict** | Çakışma (duplicate) | `ConflictException` |
| **422 Unprocessable Entity** | İş kuralı hatası | `BusinessException` |

```csharp
// Bu dönüşler ExceptionHandlingMiddleware tarafından otomatik yapılır
// Service'te exception fırlatmak yeterli:

// 404 Not Found
throw new NotFoundException($"Ürün bulunamadı: {id}");

// 400 Bad Request (Validation)
throw new ValidationException(validationResult.Errors);

// 422 Unprocessable Entity (Business Rule)
throw new BusinessException("Bu ürün silinemiyor, aktif siparişleri var");

// 409 Conflict
throw new ConflictException("Bu e-posta adresi zaten kayıtlı");
```

### 4.3 Sunucu Hataları (5xx)

| Status Code | Kullanım |
|-------------|----------|
| **500 Internal Server Error** | Beklenmeyen hata |
| **503 Service Unavailable** | Servis geçici olarak kullanılamıyor |

```csharp
// 500 hatalar otomatik olarak ExceptionHandlingMiddleware tarafından yakalanır
// Detaylar loglanır, kullanıcıya generic mesaj gösterilir
```

---

## 5. Route Naming Conventions

### 5.1 RESTful Route Kuralları

```csharp
[ApiController]
[Route("api/[controller]")]  // /api/products
public class ProductsController : ControllerBase
{
    // GET /api/products
    [HttpGet]
    public async Task<ActionResult<PagedApiResponse<ProductDto>>> GetAll([FromQuery] StockFilterRequest filter)

    // GET /api/products/123
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)

    // GET /api/products/PRD-00001
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetByCode(string code)

    // POST /api/products
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductRequest request)

    // PUT /api/products/123
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, UpdateProductRequest request)

    // DELETE /api/products/123
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)

    // GET /api/products/123/variants
    [HttpGet("{id:int}/variants")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VariantDto>>>> GetVariants(int id)

    // POST /api/products/123/variants
    [HttpPost("{id:int}/variants")]
    public async Task<ActionResult<ApiResponse<VariantDto>>> AddVariant(int id, CreateVariantRequest request)
}
```

### 5.2 Nested Resource Kuralları

```csharp
// ✅ Doğru: İlişkili kaynak için nested route
// GET /api/chat/sessions/{sessionId}/messages
[HttpGet("sessions/{sessionId:guid}/messages")]
public async Task<ActionResult<ApiResponse<IEnumerable<ChatMessageDto>>>> GetSessionMessages(Guid sessionId)

// ✅ Doğru: Action-based route
// POST /api/chat/sessions/{sessionId}/send
[HttpPost("sessions/{sessionId:guid}/send")]
public async Task<ActionResult<ApiResponse<ChatResponse>>> SendMessage(Guid sessionId, SendMessageRequest request)
```

### 5.3 Query String Kullanımı

```csharp
// ✅ Filtreleme için query string
// GET /api/products?category=shirts&minPrice=100&page=1&pageSize=20
[HttpGet]
public async Task<ActionResult<PagedApiResponse<ProductDto>>> GetProducts(
    [FromQuery] string? category,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)

// ✅ Veya DTO ile
[HttpGet]
public async Task<ActionResult<PagedApiResponse<ProductDto>>> GetProducts(
    [FromQuery] StockFilterRequest filter)
```

---

## 6. Request/Response Handling

### 6.1 Model Binding

```csharp
// ✅ Route parameter
[HttpGet("{id:int}")]
public async Task<ActionResult> Get(int id)

// ✅ Query string
[HttpGet]
public async Task<ActionResult> Search([FromQuery] string term)

// ✅ Request body
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateProductRequest request)

// ✅ Header
[HttpGet]
public async Task<ActionResult> Get([FromHeader(Name = "X-Correlation-Id")] string? correlationId)

// ✅ Kombinasyon
[HttpPut("{id:int}")]
public async Task<ActionResult> Update(
    [FromRoute] int id,
    [FromBody] UpdateProductRequest request,
    CancellationToken cancellationToken)
```

### 6.2 Validation Filter

```csharp
namespace Api.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(
                ApiErrorResponse.ValidationError(errors));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

// Program.cs'de kayıt
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
```

### 6.3 CancellationToken Kullanımı

```csharp
// ✅ Tüm async action'larda CancellationToken kullan
[HttpGet]
public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard(
    CancellationToken cancellationToken)  // ASP.NET Core otomatik inject eder
{
    var data = await _dashboardService.GetDashboardDataAsync(cancellationToken);
    return Ok(ApiResponse<DashboardResponse>.Success(data));
}
```

---

## 7. Swagger/OpenAPI Dokümantasyonu

### 7.1 Swagger Konfigürasyonu

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nebim Dashboard API",
        Version = "v1",
        Description = "Nebim V3 ERP Dashboard API",
        Contact = new OpenApiContact
        {
            Name = "Destek",
            Email = "destek@example.com"
        }
    });

    // XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // JWT Auth
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT token'ınızı girin"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nebim Dashboard API v1");
        options.RoutePrefix = string.Empty;  // Swagger'ı root'ta aç
    });
}
```

### 7.2 Controller Dokümantasyonu

```csharp
/// <summary>
/// Stok yönetimi işlemleri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Stok")]
public class StockController : ControllerBase
{
    /// <summary>
    /// Ürün listesini filtreli olarak getirir
    /// </summary>
    /// <param name="filter">Filtreleme parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalı ürün listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="400">Geçersiz filtre parametreleri</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<ProductListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedApiResponse<ProductListItemDto>>> GetProducts(
        [FromQuery] StockFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var result = await _stockService.GetProductsAsync(filter, cancellationToken);
        return Ok(PagedApiResponse<ProductListItemDto>.Success(result));
    }

    /// <summary>
    /// Belirtilen ID'ye sahip ürünün detaylarını getirir
    /// </summary>
    /// <param name="id">Ürün ID (Nebim ProductRecId)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ürün detayları</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="404">Ürün bulunamadı</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProductDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _stockService.GetProductDetailAsync(id, cancellationToken);
        return Ok(ApiResponse<ProductDetailDto>.Success(product));
    }
}
```

### 7.3 XML Comments Aktifleştirme

```xml
<!-- Api.csproj -->
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

## 8. CORS ve Güvenlik

### 8.1 CORS Konfigürasyonu

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://dashboard.example.com")
              .AllowAnyHeader()
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .AllowCredentials();
    });
});

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}
```

### 8.2 Rate Limiting

```csharp
// Program.cs (.NET 7+)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiErrorResponse.Create("Çok fazla istek gönderildi. Lütfen bekleyin.", "RATE_LIMIT_EXCEEDED"),
            token);
    };
});

// Middleware
app.UseRateLimiter();
```

### 8.3 Authorization

```csharp
// Controller seviyesinde
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Tüm action'lar için auth gerekli
}

// Action seviyesinde
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]  // Sadece Admin
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)

// Public endpoint
[HttpGet("public/status")]
[AllowAnonymous]
public ActionResult<string> GetStatus() => Ok("Healthy");
```

---

## 📝 Kontrol Listesi

API kodu yazarken şunları kontrol et:

- [ ] Controller `[ApiController]` attribute'u var mı?
- [ ] Route `api/[controller]` formatında mı?
- [ ] Controller'da iş mantığı yok mu?
- [ ] `ApiResponse<T>` wrapper kullanılıyor mu?
- [ ] `CancellationToken` parametresi eklendi mi?
- [ ] HTTP status kodları doğru mu?
- [ ] `ProducesResponseType` attribute'ları var mı?
- [ ] XML documentation yazıldı mı?
- [ ] CORS politikası doğru mu?
- [ ] Route parameter constraint'leri var mı? (`{id:int}`, `{id:guid}`)

---

*Son Güncelleme: 26 Aralık 2025*
