using System.Text.Json;
using Entity.Enums;

namespace Entity.DTOs.AI;

/// <summary>
/// Business Intelligence sorgu sonucu.
/// </summary>
public class BusinessQueryResponse
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Orijinal sorgu.
    /// </summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// Belirlenen intent.
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// AI güven skoru.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Veri kaynağı.
    /// </summary>
    public string DataSource { get; set; } = "real";

    /// <summary>
    /// Capability sonuçları.
    /// </summary>
    public List<CapabilityResultDto> Results { get; set; } = new();

    /// <summary>
    /// Birleştirilmiş/formatlanmış sonuç verisi.
    /// </summary>
    public JsonElement? AggregatedData { get; set; }

    /// <summary>
    /// İnsan okunabilir özet.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Kapsam dışı ise önerilen capability'ler.
    /// </summary>
    public List<SuggestedCapabilityDto>? Suggestions { get; set; }

    /// <summary>
    /// Upgrade gerekiyor mu?
    /// </summary>
    public bool RequiresUpgrade { get; set; }

    /// <summary>
    /// Upgrade için mesaj (varsa).
    /// </summary>
    public string? UpgradeMessage { get; set; }

    /// <summary>
    /// Toplam execution süresi (ms).
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }

    /// <summary>
    /// Bu sorgu için harcanan quota.
    /// </summary>
    public int QuotaConsumed { get; set; } = 1;

    /// <summary>
    /// Kalan günlük quota.
    /// </summary>
    public int RemainingDailyQuota { get; set; }

    /// <summary>
    /// Hata mesajı (varsa).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Hata kodu (varsa).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Response timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
