using BLL.AI.Validation;
using DAL.Context;
using System.Collections.Concurrent;

namespace Api.Middleware;

/// <summary>
/// Rate limiting middleware.
/// Tenant bazlı sorgu limitlerini uygular.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Basit in-memory rate limiter (Production için Redis kullanılmalı)
    private static readonly ConcurrentDictionary<string, RateLimitEntry> RateLimits = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        ITenantContext tenantContext,
        ISubscriptionValidator subscriptionValidator)
    {
        // Sadece BI endpoint'leri için rate limit uygula
        if (!context.Request.Path.StartsWithSegments("/api/bi"))
        {
            await _next(context);
            return;
        }

        // Tenant yoksa devam et (auth middleware handle eder)
        if (!tenantContext.TenantId.HasValue)
        {
            await _next(context);
            return;
        }

        var tenantId = tenantContext.TenantId.Value;
        var key = $"tenant:{tenantId}";

        // Sliding window rate limit (dakikalık)
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-1);

        var entry = RateLimits.GetOrAdd(key, _ => new RateLimitEntry());

        bool rateLimitExceeded;
        int limit;
        int currentCount;
        
        await entry.Semaphore.WaitAsync();
        try
        {
            // Eski istekleri temizle
            entry.Requests.RemoveAll(r => r < windowStart);

            // Dakikalık limit kontrolü (tier'a göre)
            limit = GetRateLimitByTier(tenantContext.SubscriptionTier);
            currentCount = entry.Requests.Count;

            if (currentCount >= limit)
            {
                rateLimitExceeded = true;
            }
            else
            {
                rateLimitExceeded = false;
                // İsteği kaydet
                entry.Requests.Add(now);
                currentCount = entry.Requests.Count;
            }
        }
        finally
        {
            entry.Semaphore.Release();
        }

        if (rateLimitExceeded)
        {
            _logger.LogWarning(
                "Rate limit exceeded for tenant {TenantId}. Requests: {Count}/{Limit}",
                tenantId, currentCount, limit);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                errorCode = "RATE_LIMIT_EXCEEDED",
                message = $"Dakikalık istek limitine ulaşıldı ({limit}/dakika). Lütfen bekleyin.",
                retryAfter = 60
            });

            return;
        }

        // Response headers ekle
        var remaining = GetRateLimitByTier(tenantContext.SubscriptionTier) - currentCount;
        context.Response.Headers["X-RateLimit-Limit"] = GetRateLimitByTier(tenantContext.SubscriptionTier).ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remaining).ToString();

        await _next(context);
    }

    private static int GetRateLimitByTier(Entity.Enums.SubscriptionTier tier)
    {
        return tier switch
        {
            Entity.Enums.SubscriptionTier.Free => 10, // 10 istek/dakika
            Entity.Enums.SubscriptionTier.Professional => 30, // 30 istek/dakika
            Entity.Enums.SubscriptionTier.Enterprise => 100, // 100 istek/dakika
            _ => 10
        };
    }

    private class RateLimitEntry
    {
        public List<DateTime> Requests { get; } = new();
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }
}

/// <summary>
/// Middleware extension.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
