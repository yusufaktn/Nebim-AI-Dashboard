using DAL.Context;
using Entity.App;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Validation;

/// <summary>
/// Subscription kota doğrulayıcı implementasyonu.
/// </summary>
public class SubscriptionValidator : ISubscriptionValidator
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SubscriptionValidator> _logger;

    public SubscriptionValidator(
        AppDbContext dbContext,
        ILogger<SubscriptionValidator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<QuotaCheckResult> CheckQuotaAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found for quota check", tenantId);
            return QuotaCheckResult.Exceeded("Tenant bulunamadı", DateTime.UtcNow);
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is inactive", tenantId);
            return QuotaCheckResult.Exceeded("Tenant aktif değil", DateTime.UtcNow);
        }

        var plan = tenant.SubscriptionPlan;
        if (plan == null)
        {
            _logger.LogWarning("Tenant {TenantId} has no subscription plan", tenantId);
            return QuotaCheckResult.Exceeded("Subscription planı bulunamadı", DateTime.UtcNow);
        }

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Günlük kota kontrolü
        var dailyQuota = await GetOrCreateQuotaAsync(tenantId, todayStart, plan.DailyQueryLimit, QuotaPeriodType.Daily, ct);
        
        // Aylık kota kontrolü
        var monthlyQuota = await GetOrCreateQuotaAsync(tenantId, monthStart, plan.MonthlyQueryLimit, QuotaPeriodType.Monthly, ct);

        var remainingDailyQueries = dailyQuota.RemainingQueries;
        var remainingMonthlyQueries = monthlyQuota.RemainingQueries;

        if (remainingDailyQueries <= 0)
        {
            var resetTime = todayStart.AddDays(1);
            _logger.LogInformation("Daily quota exceeded for tenant {TenantId}", tenantId);
            return QuotaCheckResult.Exceeded(
                $"Günlük sorgu limitine ulaşıldı ({plan.DailyQueryLimit}). Yarın sıfırlanacak.",
                resetTime);
        }

        if (remainingMonthlyQueries <= 0)
        {
            var resetTime = monthStart.AddMonths(1);
            _logger.LogInformation("Monthly quota exceeded for tenant {TenantId}", tenantId);
            return QuotaCheckResult.Exceeded(
                $"Aylık sorgu limitine ulaşıldı ({plan.MonthlyQueryLimit}). Gelecek ay sıfırlanacak.",
                resetTime);
        }

        return QuotaCheckResult.Allowed(
            Math.Min(remainingDailyQueries, remainingMonthlyQueries),
            int.MaxValue); // Token limit yok
    }

    public async Task ConsumeQuotaAsync(int tenantId, int tokensUsed, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Günlük kota güncelle
        var dailyQuota = await _dbContext.QueryQuotas
            .FirstOrDefaultAsync(q => q.TenantId == tenantId && 
                                      q.PeriodStart == todayStart && 
                                      q.PeriodType == QuotaPeriodType.Daily, ct);

        if (dailyQuota != null)
        {
            dailyQuota.UsedQueries++;
            dailyQuota.LastUpdatedAt = now;
        }

        // Aylık kota güncelle
        var monthlyQuota = await _dbContext.QueryQuotas
            .FirstOrDefaultAsync(q => q.TenantId == tenantId && 
                                      q.PeriodStart == monthStart && 
                                      q.PeriodType == QuotaPeriodType.Monthly, ct);

        if (monthlyQuota != null)
        {
            monthlyQuota.UsedQueries++;
            monthlyQuota.LastUpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogDebug(
            "Quota consumed for tenant {TenantId}. Daily: {Daily}, Monthly: {Monthly}",
            tenantId, 
            dailyQuota?.UsedQueries ?? 0, 
            monthlyQuota?.UsedQueries ?? 0);
    }

    private async Task<QueryQuota> GetOrCreateQuotaAsync(
        int tenantId, 
        DateTime periodStart, 
        int limit, 
        QuotaPeriodType periodType,
        CancellationToken ct)
    {
        var quota = await _dbContext.QueryQuotas
            .FirstOrDefaultAsync(q => q.TenantId == tenantId && 
                                      q.PeriodStart == periodStart && 
                                      q.PeriodType == periodType, ct);

        if (quota == null)
        {
            var periodEnd = periodType == QuotaPeriodType.Daily 
                ? periodStart.AddDays(1).AddSeconds(-1)
                : periodStart.AddMonths(1).AddSeconds(-1);

            quota = new QueryQuota
            {
                TenantId = tenantId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PeriodType = periodType,
                TotalLimit = limit,
                UsedQueries = 0,
                LastUpdatedAt = DateTime.UtcNow
            };

            _dbContext.QueryQuotas.Add(quota);
            await _dbContext.SaveChangesAsync(ct);
        }

        return quota;
    }
}
