namespace Entity.Exceptions;

/// <summary>
/// Tüm uygulama exception'larının base sınıfı
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// HTTP Status Code
    /// </summary>
    public abstract int StatusCode { get; }
    
    /// <summary>
    /// Hata kodu (loglama ve takip için)
    /// </summary>
    public string ErrorCode { get; }
    
    protected AppException(string message, string? errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode ?? GetType().Name;
    }

    protected AppException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? GetType().Name;
    }
}

/// <summary>
/// Validation hatası (400 Bad Request)
/// </summary>
public class ValidationException : AppException
{
    public override int StatusCode => 400;
    
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(string message) 
        : base(message, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("Doğrulama hatası oluştu", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
    
    public ValidationException(string field, string error) 
        : base($"Doğrulama hatası: {error}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}

/// <summary>
/// Kayıt bulunamadı hatası (404 Not Found)
/// </summary>
public class NotFoundException : AppException
{
    public override int StatusCode => 404;
    
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} bulunamadı. Anahtar: {key}", "NOT_FOUND")
    {
    }
    
    public NotFoundException(string message) 
        : base(message, "NOT_FOUND")
    {
    }
}

/// <summary>
/// Yetkilendirme hatası (401 Unauthorized)
/// </summary>
public class UnauthorizedException : AppException
{
    public override int StatusCode => 401;
    
    public UnauthorizedException(string message = "Yetkisiz erişim") 
        : base(message, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Erişim yasak hatası (403 Forbidden)
/// </summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => 403;
    
    public ForbiddenException(string message = "Bu işlem için yetkiniz yok") 
        : base(message, "FORBIDDEN")
    {
    }
}

/// <summary>
/// İş kuralı hatası (422 Unprocessable Entity)
/// </summary>
public class BusinessException : AppException
{
    public override int StatusCode => 422;
    
    public BusinessException(string message, string? errorCode = null) 
        : base(message, errorCode ?? "BUSINESS_ERROR")
    {
    }
}

/// <summary>
/// Çakışma hatası (409 Conflict)
/// </summary>
public class ConflictException : AppException
{
    public override int StatusCode => 409;
    
    public ConflictException(string message) 
        : base(message, "CONFLICT")
    {
    }
    
    public ConflictException(string entityName, string field, object value) 
        : base($"{entityName} zaten mevcut. {field}: {value}", "CONFLICT")
    {
    }
}

/// <summary>
/// Harici servis hatası (502 Bad Gateway)
/// </summary>
public class ExternalServiceException : AppException
{
    public override int StatusCode => 502;
    
    public string ServiceName { get; }
    
    public ExternalServiceException(string serviceName, string message) 
        : base($"{serviceName} servisinde hata: {message}", "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }
    
    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"{serviceName} servisinde hata: {message}", innerException, "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }
}
