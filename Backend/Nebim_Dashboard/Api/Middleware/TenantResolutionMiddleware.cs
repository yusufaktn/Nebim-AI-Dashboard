using DAL.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Middleware;

/// <summary>
/// Tenant çözümleme middleware'i.
/// JWT token'dan tenant bilgisini çıkarır ve TenantContext'e set eder.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, AppDbContext dbContext)
    {
        // Authentication gerekli endpoint'ler için tenant'ı çöz
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id");
            var userIdClaim = context.User.FindFirst("user_id") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                // Tenant'ı doğrula
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .Include(t => t.SubscriptionPlan)
                    .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

                if (tenant != null)
                {
                    tenantContext.TenantId = tenantId;
                    tenantContext.TenantName = tenant.Name;
                    tenantContext.SubscriptionTier = tenant.SubscriptionPlan?.Tier ?? Entity.Enums.SubscriptionTier.Free;

                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                    {
                        tenantContext.UserId = userId;
                    }

                    // Tenant bilgisini request items'a da ekle (opsiyonel)
                    context.Items["TenantId"] = tenantId;
                    context.Items["TenantName"] = tenant.Name;
                    context.Items["SubscriptionTier"] = tenant.SubscriptionPlan?.Tier;

                    _logger.LogDebug(
                        "Tenant resolved: {TenantId} - {TenantName}",
                        tenantId, tenant.Name);
                }
                else
                {
                    _logger.LogWarning(
                        "Invalid or inactive tenant: {TenantId}",
                        tenantId);
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Middleware extension.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
