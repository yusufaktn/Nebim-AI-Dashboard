namespace Entity.DTOs.Common;

/// <summary>
/// RFC 7807 Problem Details standardına uygun hata modeli
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// Hata tipi URI
    /// </summary>
    public string Type { get; set; } = "about:blank";
    
    /// <summary>
    /// Kısa hata başlığı
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    /// Detaylı hata açıklaması
    /// </summary>
    public string? Detail { get; set; }
    
    /// <summary>
    /// Hata oluşan endpoint
    /// </summary>
    public string? Instance { get; set; }
    
    /// <summary>
    /// Correlation ID (request takibi için)
    /// </summary>
    public string? TraceId { get; set; }
    
    /// <summary>
    /// Ek hata bilgileri
    /// </summary>
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Validation hatası için genişletilmiş problem details
/// </summary>
public class ValidationProblemDetails : ProblemDetails
{
    public ValidationProblemDetails()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        Title = "Doğrulama hatası oluştu";
        Status = 400;
    }
    
    /// <summary>
    /// Validation hataları (alan adı -> hata mesajları)
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
