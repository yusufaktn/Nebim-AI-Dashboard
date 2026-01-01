using Entity.Base;

namespace Entity.App;

/// <summary>
/// Tenant'ın belirli bir dönem için query quota'sı.
/// </summary>
public class QueryQuota : BaseEntity
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Quota dönemi başlangıcı.
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Quota dönemi bitişi.
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Dönem tipi.
    /// </summary>
    public QuotaPeriodType PeriodType { get; set; }

    /// <summary>
    /// Dönem için toplam limit.
    /// </summary>
    public int TotalLimit { get; set; }

    /// <summary>
    /// Kullanılan sorgu sayısı.
    /// </summary>
    public int UsedQueries { get; set; } = 0;

    /// <summary>
    /// Kalan sorgu sayısı.
    /// </summary>
    public int RemainingQueries => TotalLimit - UsedQueries;

    /// <summary>
    /// Limit aşıldı mı?
    /// </summary>
    public bool IsExceeded => UsedQueries >= TotalLimit && TotalLimit > 0;

    /// <summary>
    /// Kullanım yüzdesi.
    /// </summary>
    public double UsagePercentage => TotalLimit > 0 ? (double)UsedQueries / TotalLimit * 100 : 0;

    /// <summary>
    /// Son güncelleme zamanı.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== Navigation Properties =====

    /// <summary>
    /// Tenant.
    /// </summary>
    public virtual Tenant Tenant { get; set; } = null!;
}

/// <summary>
/// Quota dönem tipi.
/// </summary>
public enum QuotaPeriodType
{
    /// <summary>
    /// Günlük quota.
    /// </summary>
    Daily = 0,

    /// <summary>
    /// Aylık quota.
    /// </summary>
    Monthly = 1
}
