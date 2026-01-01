using System.Diagnostics;
using Api.Common;
using Entity.Exceptions;

namespace Api.Middleware;

/// <summary>
/// Global Exception Handling Middleware
/// 
/// 🎓 AÇIKLAMA:
/// - Tüm unhandled exception'ları yakalar
/// - Exception tipine göre uygun HTTP status code döner
/// - Development'ta detaylı hata, Production'da generic mesaj
/// - Her hata için TraceId oluşturur (debugging için)
/// </summary>
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

        // 🎓 Pattern Matching: Exception tipine göre farklı response
        var (statusCode, response) = exception switch
        {
            // Validation hatası (400)
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ApiErrorResponse
                {
                    Message = validationEx.Message,
                    ErrorCode = "VALIDATION_ERROR",
                    Errors = validationEx.Errors,
                    TraceId = traceId
                }),

            // Yetkilendirme hatası (401)
            UnauthorizedException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                new ApiErrorResponse
                {
                    Message = unauthorizedEx.Message,
                    ErrorCode = "UNAUTHORIZED",
                    TraceId = traceId
                }),

            // Yetki yetersiz (403)
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ApiErrorResponse
                {
                    Message = forbiddenEx.Message,
                    ErrorCode = "FORBIDDEN",
                    TraceId = traceId
                }),

            // Bulunamadı (404)
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ApiErrorResponse
                {
                    Message = notFoundEx.Message,
                    ErrorCode = "NOT_FOUND",
                    TraceId = traceId
                }),

            // Çakışma (409)
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                new ApiErrorResponse
                {
                    Message = conflictEx.Message,
                    ErrorCode = "CONFLICT",
                    TraceId = traceId
                }),

            // İş kuralı ihlali (422)
            BusinessException businessEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ApiErrorResponse
                {
                    Message = businessEx.Message,
                    ErrorCode = "BUSINESS_RULE_VIOLATION",
                    TraceId = traceId
                }),

            // İstek iptal edildi (499)
            OperationCanceledException => (
                499, // Client Closed Request
                new ApiErrorResponse
                {
                    Message = "İstek iptal edildi",
                    ErrorCode = "REQUEST_CANCELLED",
                    TraceId = traceId
                }),

            // Diğer tüm hatalar (500)
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
        // 🎓 Development'ta detaylı hata göster
        if (_environment.IsDevelopment())
        {
            return new ApiErrorResponse
            {
                Message = exception.Message,
                ErrorCode = "INTERNAL_ERROR",
                TraceId = traceId,
                Errors = new Dictionary<string, string[]>
                {
                    ["stackTrace"] = [exception.StackTrace ?? ""],
                    ["exceptionType"] = [exception.GetType().Name]
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
        // 🎓 Log Seviyeleri:
        // - 5xx = Error (sunucu hatası, acil müdahale gerekebilir)
        // - 4xx = Warning (client hatası, normal durum)
        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Sunucu hatası. TraceId: {TraceId}, StatusCode: {StatusCode}",
                traceId,
                statusCode);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "İstemci hatası. TraceId: {TraceId}, StatusCode: {StatusCode}, Message: {Message}",
                traceId,
                statusCode,
                exception.Message);
        }
    }
}
