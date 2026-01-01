using Entity.DTOs.AI;
using Entity.Enums;

namespace BLL.AI.Validation;

/// <summary>
/// Query plan doğrulayıcı interface.
/// </summary>
public interface IQueryPlanValidator
{
    /// <summary>
    /// Query plan'ı doğrular.
    /// </summary>
    Task<QueryValidationResult> ValidateAsync(QueryPlanDto plan, int tenantId, SubscriptionTier tier, CancellationToken ct = default);
}

/// <summary>
/// Subscription doğrulayıcı interface.
/// </summary>
public interface ISubscriptionValidator
{
    /// <summary>
    /// Tenant'ın subscription kotasını kontrol eder.
    /// </summary>
    Task<QuotaCheckResult> CheckQuotaAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Sorgu kotasını günceller (sorgu sonrası).
    /// </summary>
    Task ConsumeQuotaAsync(int tenantId, int tokensUsed, CancellationToken ct = default);
}

/// <summary>
/// Tenant doğrulayıcı interface.
/// </summary>
public interface ITenantValidator
{
    /// <summary>
    /// Tenant'ın aktif ve geçerli olduğunu kontrol eder.
    /// </summary>
    Task<TenantValidationResult> ValidateAsync(int tenantId, CancellationToken ct = default);
}

/// <summary>
/// Query plan doğrulama sonucu.
/// </summary>
public class QueryValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> RestrictedCapabilities { get; set; } = new();

    public static QueryValidationResult Success() => new() { IsValid = true };
    
    public static QueryValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    public static QueryValidationResult WithWarnings(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = warnings.ToList()
    };
}

/// <summary>
/// Kota kontrolü sonucu.
/// </summary>
public class QuotaCheckResult
{
    public bool HasQuota { get; set; }
    public int RemainingQueries { get; set; }
    public int RemainingTokens { get; set; }
    public DateTime? ResetTime { get; set; }
    public string? Message { get; set; }

    public static QuotaCheckResult Allowed(int remainingQueries, int remainingTokens) => new()
    {
        HasQuota = true,
        RemainingQueries = remainingQueries,
        RemainingTokens = remainingTokens
    };

    public static QuotaCheckResult Exceeded(string message, DateTime resetTime) => new()
    {
        HasQuota = false,
        Message = message,
        ResetTime = resetTime
    };
}

/// <summary>
/// Tenant doğrulama sonucu.
/// </summary>
public class TenantValidationResult
{
    public bool IsValid { get; set; }
    public int TenantId { get; set; }
    public SubscriptionTier Tier { get; set; }
    public bool NebimConnected { get; set; }
    public string? Error { get; set; }

    public static TenantValidationResult Success(int tenantId, SubscriptionTier tier, bool nebimConnected) => new()
    {
        IsValid = true,
        TenantId = tenantId,
        Tier = tier,
        NebimConnected = nebimConnected
    };

    public static TenantValidationResult Failure(string error) => new()
    {
        IsValid = false,
        Error = error
    };
}
