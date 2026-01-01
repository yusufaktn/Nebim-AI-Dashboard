using System.Diagnostics;

namespace Api.Common;

/// <summary>
/// Standart API response wrapper
/// 
/// 🎓 AÇIKLAMA:
/// Tüm API yanıtları bu wrapper ile sarılır:
/// - Tutarlılık: Frontend her zaman aynı format bekler
/// - IsSuccess: İşlem başarılı mı?
/// - Data: Gerçek veri
/// - Message: Opsiyonel kullanıcı mesajı
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
}

/// <summary>
/// Sayfalı response wrapper
/// </summary>
public class PagedApiResponse<T>
{
    public bool IsSuccess { get; set; } = true;
    public List<T> Items { get; set; } = [];
    public PaginationMeta Pagination { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
