# 🚨 Hata Yönetimi Standartları

> Bu doküman, projedeki exception handling stratejisi, custom exception tipleri, global error middleware ve kullanıcı dostu hata mesajlarını tanımlar.

---

## 📌 İçindekiler

1. [Exception Hiyerarşisi](#1-exception-hiyerarşisi)
2. [Custom Exception Tipleri](#2-custom-exception-tipleri)
3. [Global Exception Middleware](#3-global-exception-middleware)
4. [Hata Fırlatma Kuralları](#4-hata-fırlatma-kuralları)
5. [Validation Hataları](#5-validation-hataları)
6. [Problem Details Standardı](#6-problem-details-standardı)
7. [Kullanıcı Dostu Mesajlar](#7-kullanıcı-dostu-mesajlar)

---

## 1. Exception Hiyerarşisi

### 1.1 Exception Yapısı

```
Exception (System)
└── ApplicationException (System)
    └── AppException (Base class)
        ├── ValidationException       → 400 Bad Request
        ├── UnauthorizedException     → 401 Unauthorized
        ├── ForbiddenException        → 403 Forbidden
        ├── NotFoundException         → 404 Not Found
        ├── ConflictException         → 409 Conflict
        └── BusinessException         → 422 Unprocessable Entity
```

### 1.2 Klasör Yapısı

```
Entity/
└── Exceptions/
    ├── AppException.cs           # Base exception
    ├── ValidationException.cs
    ├── UnauthorizedException.cs
    ├── ForbiddenException.cs
    ├── NotFoundException.cs
    ├── ConflictException.cs
    └── BusinessException.cs

Api/
└── Middleware/
    └── ExceptionHandlingMiddleware.cs
```

---

## 2. Custom Exception Tipleri

### 2.1 Base Exception

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Tüm uygulama exception'larının base class'ı
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// HTTP status kodu
    /// </summary>
    public abstract int StatusCode { get; }
    
    /// <summary>
    /// Hata kodu (loglama ve tracking için)
    /// </summary>
    public virtual string ErrorCode => GetType().Name.Replace("Exception", "").ToUpperInvariant();

    protected AppException(string message) : base(message) { }
    
    protected AppException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### 2.2 Validation Exception

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Doğrulama hataları için exception (400 Bad Request)
/// </summary>
public class ValidationException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string ErrorCode => "VALIDATION_ERROR";
    
    /// <summary>
    /// Alan bazlı hata mesajları
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("Bir veya daha fazla doğrulama hatası oluştu")
    {
        Errors = errors;
    }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("Bir veya daha fazla doğrulama hatası oluştu")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray()
            );
    }
}
```

### 2.3 NotFoundException

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Kayıt bulunamadığında fırlatılır (404 Not Found)
/// </summary>
public class NotFoundException : AppException
{
    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string ErrorCode => "NOT_FOUND";

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} bulunamadı: {key}") { }

    public static NotFoundException ForEntity<T>(object key) 
        => new(typeof(T).Name, key);
}
```

### 2.4 BusinessException

```csharp
namespace Entity.Exceptions;

/// <summary>
/// İş kuralı ihlali durumunda fırlatılır (422 Unprocessable Entity)
/// </summary>
public class BusinessException : AppException
{
    public override int StatusCode => StatusCodes.Status422UnprocessableEntity;
    public override string ErrorCode => "BUSINESS_RULE_VIOLATION";

    public BusinessException(string message) : base(message) { }

    public BusinessException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### 2.5 UnauthorizedException

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Kimlik doğrulama gerektiğinde fırlatılır (401 Unauthorized)
/// </summary>
public class UnauthorizedException : AppException
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public override string ErrorCode => "UNAUTHORIZED";

    public UnauthorizedException() : base("Kimlik doğrulama gerekli") { }
    
    public UnauthorizedException(string message) : base(message) { }
}
```

### 2.6 ForbiddenException

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Yetki yetersiz olduğunda fırlatılır (403 Forbidden)
/// </summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string ErrorCode => "FORBIDDEN";

    public ForbiddenException() : base("Bu işlem için yetkiniz yok") { }
    
    public ForbiddenException(string message) : base(message) { }
}
```

### 2.7 ConflictException

```csharp
namespace Entity.Exceptions;

/// <summary>
/// Kaynak çakışması durumunda fırlatılır (409 Conflict)
/// </summary>
public class ConflictException : AppException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string ErrorCode => "CONFLICT";

    public ConflictException(string message) : base(message) { }

    public static ConflictException DuplicateEntry(string field, object value)
        => new($"'{field}' alanı için '{value}' değeri zaten mevcut");
}
```

---

## 3. Global Exception Middleware

### 3.1 Middleware Implementasyonu

```csharp
namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Exception tipine göre response oluştur
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                validationEx.StatusCode,
                new ApiErrorResponse
                {
                    Message = validationEx.Message,
                    ErrorCode = validationEx.ErrorCode,
                    Errors = validationEx.Errors,
                    TraceId = traceId
                }),

            AppException appEx => (
                appEx.StatusCode,
                new ApiErrorResponse
                {
                    Message = appEx.Message,
                    ErrorCode = appEx.ErrorCode,
                    TraceId = traceId
                }),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                new ApiErrorResponse
                {
                    Message = "İstek iptal edildi",
                    ErrorCode = "REQUEST_CANCELLED",
                    TraceId = traceId
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                CreateInternalErrorResponse(exception, traceId))
        };

        // Loglama
        LogException(exception, statusCode, traceId);

        // Response yaz
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(response);
    }

    private ApiErrorResponse CreateInternalErrorResponse(Exception exception, string traceId)
    {
        // Development ortamında detaylı hata
        if (_environment.IsDevelopment())
        {
            return new ApiErrorResponse
            {
                Message = exception.Message,
                ErrorCode = "INTERNAL_ERROR",
                TraceId = traceId,
                // Development'ta stack trace göster
                Errors = new Dictionary<string, string[]>
                {
                    ["stackTrace"] = [exception.StackTrace ?? ""]
                }
            };
        }

        // Production'da generic mesaj
        return new ApiErrorResponse
        {
            Message = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
            ErrorCode = "INTERNAL_ERROR",
            TraceId = traceId
        };
    }

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        // 4xx hataları Warning, 5xx hataları Error olarak logla
        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}, StatusCode: {StatusCode}",
                traceId,
                statusCode);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "Client error occurred. TraceId: {TraceId}, StatusCode: {StatusCode}, Message: {Message}",
                traceId,
                statusCode,
                exception.Message);
        }
    }
}
```

### 3.2 Middleware Kaydı

```csharp
// Program.cs
var app = builder.Build();

// ✅ Exception middleware en başta olmalı
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Diğer middleware'ler
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 3.3 Extension Method

```csharp
// Extensions/ApplicationBuilderExtensions.cs
namespace Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

// Program.cs
app.UseGlobalExceptionHandler();
```

---

## 4. Hata Fırlatma Kuralları

### 4.1 Nerede Exception Fırlatılır?

| Katman | Exception Türleri | Örnek |
|--------|-------------------|-------|
| **DAL** | Genellikle fırlatmaz | Null döner, BLL kontrol eder |
| **BLL** | Tüm tipler | `NotFoundException`, `BusinessException` |
| **API** | Fırlatmaz | Middleware yakalar |

### 4.2 Doğru Exception Fırlatma

```csharp
// ✅ BLL Service'te exception fırlatma
public class UserService : IUserService
{
    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct);
        
        // ✅ Kayıt yoksa NotFoundException
        if (user is null)
        {
            throw new NotFoundException("Kullanıcı", id);
            // veya: throw NotFoundException.ForEntity<User>(id);
        }

        return user.ToDto();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        // ✅ Validation
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // ✅ İş kuralı kontrolü
        if (await _unitOfWork.Users.ExistsAsync(request.Email, ct))
        {
            throw ConflictException.DuplicateEntry("Email", request.Email);
        }

        var user = new User { /* ... */ };
        await _unitOfWork.Users.CreateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return user.ToDto();
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Kullanıcı", id);

        // ✅ İş kuralı ihlali
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _unitOfWork.Users.CountByRoleAsync(UserRole.Admin, ct);
            if (adminCount <= 1)
            {
                throw new BusinessException("Son admin kullanıcısı silinemez");
            }
        }

        await _unitOfWork.Users.DeleteAsync(id, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

### 4.3 Exception Fırlatma Anti-Pattern'leri

```csharp
// ❌ YANLIŞ: Flow control için exception kullanma
public async Task<bool> UserExistsAsync(string email)
{
    try
    {
        await GetUserByEmailAsync(email);
        return true;
    }
    catch (NotFoundException)
    {
        return false;  // ❌ Exception'ı flow control için kullandık
    }
}

// ✅ DOĞRU: Boolean dönen method kullan
public async Task<bool> UserExistsAsync(string email, CancellationToken ct)
{
    return await _unitOfWork.Users.ExistsAsync(email, ct);
}


// ❌ YANLIŞ: Generic exception fırlatma
throw new Exception("Bir hata oluştu");  // ❌

// ✅ DOĞRU: Spesifik exception kullan
throw new BusinessException("Stok yetersiz, sipariş oluşturulamıyor");


// ❌ YANLIŞ: Exception'ı yutma
try
{
    await ProcessOrderAsync();
}
catch (Exception)
{
    // Hiçbir şey yapma  ❌
}

// ✅ DOĞRU: Logla ve tekrar fırlat (veya handle et)
try
{
    await ProcessOrderAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Order processing failed");
    throw;  // Veya uygun şekilde handle et
}
```

---

## 5. Validation Hataları

### 5.1 FluentValidation ile Validation Exception

```csharp
// BLL Service
public class ProductService : IProductService
{
    private readonly IValidator<CreateProductRequest> _validator;

    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request, 
        CancellationToken ct)
    {
        // ✅ Validation kontrolü
        var result = await _validator.ValidateAsync(request, ct);
        
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        // Devam...
    }
}
```

### 5.2 Response Formatı

```json
{
    "isSuccess": false,
    "message": "Bir veya daha fazla doğrulama hatası oluştu",
    "errorCode": "VALIDATION_ERROR",
    "errors": {
        "email": [
            "E-posta adresi geçersiz",
            "E-posta adresi en fazla 150 karakter olabilir"
        ],
        "password": [
            "Şifre en az 8 karakter olmalıdır",
            "Şifre en az bir büyük harf içermelidir"
        ]
    },
    "traceId": "00-abc123...",
    "timestamp": "2025-12-26T14:30:00Z"
}
```

### 5.3 Validation Filter (Otomatik)

```csharp
// Filters/ValidationFilter.cs
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => ToCamelCase(e.Key),
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }

        await next();
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}

// Program.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
```

---

## 6. Problem Details Standardı

### 6.1 RFC 7807 Problem Details

```csharp
// Alternatif: ProblemDetails kullanımı (Microsoft.AspNetCore.Mvc)
public class ExceptionHandlingMiddleware
{
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Doğrulama Hatası",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            },

            NotFoundException notFoundEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Kayıt Bulunamadı",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundEx.Message,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Sunucu Hatası",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _environment.IsDevelopment() ? exception.Message : "Beklenmeyen bir hata oluştu",
                Instance = context.Request.Path
            }
        };

        // TraceId ekle
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";
        
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

### 6.2 Problem Details Response Örneği

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Kayıt Bulunamadı",
    "status": 404,
    "detail": "Kullanıcı bulunamadı: 123e4567-e89b-12d3-a456-426614174000",
    "instance": "/api/users/123e4567-e89b-12d3-a456-426614174000",
    "traceId": "00-abc123def456..."
}
```

---

## 7. Kullanıcı Dostu Mesajlar

### 7.1 Mesaj Kuralları

| ✅ Doğru | ❌ Yanlış |
|----------|-----------|
| "E-posta adresi zaten kayıtlı" | "Duplicate key violation in Users table" |
| "Ürün bulunamadı" | "NullReferenceException at line 42" |
| "Şifre en az 8 karakter olmalıdır" | "Password validation failed" |
| "Stok yetersiz, sipariş oluşturulamıyor" | "BusinessLogicException: insufficient_stock" |

### 7.2 Türkçe Hata Mesajları

```csharp
// Entity/Resources/ErrorMessages.cs
public static class ErrorMessages
{
    // Genel
    public const string UnexpectedError = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
    public const string RequestCancelled = "İstek iptal edildi.";
    
    // Auth
    public const string InvalidCredentials = "E-posta veya şifre hatalı.";
    public const string SessionExpired = "Oturumunuz sona erdi. Lütfen tekrar giriş yapın.";
    public const string InsufficientPermission = "Bu işlem için yetkiniz bulunmuyor.";
    
    // Validation
    public const string RequiredField = "{0} alanı zorunludur.";
    public const string InvalidEmail = "Geçerli bir e-posta adresi giriniz.";
    public const string PasswordTooShort = "Şifre en az {0} karakter olmalıdır.";
    public const string MaxLengthExceeded = "{0} en fazla {1} karakter olabilir.";
    
    // Business
    public const string DuplicateEmail = "Bu e-posta adresi zaten kayıtlı.";
    public const string LastAdminCannotBeDeleted = "Son admin kullanıcısı silinemez.";
    public const string InsufficientStock = "Stok yetersiz. Mevcut: {0}, İstenen: {1}";
    
    // Not Found
    public const string EntityNotFound = "{0} bulunamadı.";
    public const string UserNotFound = "Kullanıcı bulunamadı.";
    public const string ProductNotFound = "Ürün bulunamadı.";
}

// Kullanım
throw new NotFoundException(string.Format(ErrorMessages.EntityNotFound, "Ürün"));
throw new BusinessException(string.Format(ErrorMessages.InsufficientStock, available, requested));
```

### 7.3 Teknik Detayları Gizleme

```csharp
// ✅ Production'da teknik detay gizle
private ApiErrorResponse CreateInternalErrorResponse(Exception exception, string traceId)
{
    if (_environment.IsDevelopment())
    {
        // Development: Full detay
        return new ApiErrorResponse
        {
            Message = exception.Message,
            ErrorCode = "INTERNAL_ERROR",
            TraceId = traceId,
            Errors = new Dictionary<string, string[]>
            {
                ["exception"] = [exception.GetType().Name],
                ["stackTrace"] = [exception.StackTrace ?? ""]
            }
        };
    }

    // Production: Generic mesaj + TraceId (destek için)
    return new ApiErrorResponse
    {
        Message = ErrorMessages.UnexpectedError,
        ErrorCode = "INTERNAL_ERROR",
        TraceId = traceId  // Bu ID ile logları arayabiliriz
    };
}
```

---

## 📝 Kontrol Listesi

Hata yönetimi yazarken şunları kontrol et:

- [ ] Custom exception `AppException`'dan mı türüyor?
- [ ] Exception doğru HTTP status kodunu mu döndürüyor?
- [ ] Kullanıcıya teknik detay gösterilmiyor mu?
- [ ] Exception fırlatmadan önce loglandı mı?
- [ ] Validation hataları alan bazlı mı dönüyor?
- [ ] TraceId response'a ekleniyor mu?
- [ ] Flow control için exception kullanılmamış mı?
- [ ] Generic exception yerine spesifik tip mi kullanılıyor?
- [ ] Exception mesajları Türkçe ve anlaşılır mı?
- [ ] Production'da stack trace gizleniyor mu?

---

*Son Güncelleme: 26 Aralık 2025*
