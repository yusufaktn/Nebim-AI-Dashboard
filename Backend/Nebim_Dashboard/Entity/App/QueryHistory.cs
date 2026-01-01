using Entity.Base;
using Entity.Enums;

namespace Entity.App;

/// <summary>
/// Business Intelligence sorgu geçmişi.
/// Audit trail ve analytics için.
/// </summary>
public class QueryHistory : BaseEntity
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Sorguyu yapan kullanıcı ID.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Orijinal doğal dil sorgusu.
    /// </summary>
    public required string RawQuery { get; set; }

    /// <summary>
    /// AI tarafından üretilen query plan (JSON).
    /// </summary>
    public string? ParsedPlanJson { get; set; }

    /// <summary>
    /// Belirlenen intent.
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// AI güven skoru.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Çalıştırılan capability'ler (virgülle ayrılmış).
    /// </summary>
    public string? ExecutedCapabilities { get; set; }

    /// <summary>
    /// Capability version'ları (JSON).
    /// </summary>
    public string? CapabilityVersionsJson { get; set; }

    /// <summary>
    /// Response özeti (kısa).
    /// </summary>
    public string? ResponseSummary { get; set; }

    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Hata mesajı (varsa).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Toplam execution süresi (ms).
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// AI için harcanan token sayısı.
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Veri kaynağı.
    /// </summary>
    public string DataSource { get; set; } = "real"; // "real" veya "simulation"

    /// <summary>
    /// Sorgu için harcanan quota.
    /// </summary>
    public int QuotaConsumed { get; set; } = 1;

    /// <summary>
    /// Client IP adresi.
    /// </summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// User agent.
    /// </summary>
    public string? UserAgent { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Tenant.
    /// </summary>
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Kullanıcı.
    /// </summary>
    public virtual User User { get; set; } = null!;
}
