using Entity.Base;
using Entity.Enums;

namespace Entity.App;

/// <summary>
/// Subscription (abonelik) planı tanımı.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Plan adı.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Plan açıklaması.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Plan tier'ı.
    /// </summary>
    public SubscriptionTier Tier { get; set; }

    /// <summary>
    /// Günlük sorgu limiti.
    /// 0 = sınırsız.
    /// </summary>
    public int DailyQueryLimit { get; set; } = 10;

    /// <summary>
    /// Aylık sorgu limiti.
    /// 0 = sınırsız.
    /// </summary>
    public int MonthlyQueryLimit { get; set; } = 300;

    /// <summary>
    /// Maksimum eşzamanlı sorgu sayısı.
    /// </summary>
    public int MaxConcurrentQueries { get; set; } = 1;

    /// <summary>
    /// Gerçek Nebim bağlantısına izin var mı?
    /// </summary>
    public bool AllowRealNebimConnection { get; set; } = false;

    /// <summary>
    /// Erişilebilir capability'ler (JSON array).
    /// Boş = tüm capability'ler.
    /// </summary>
    public string? AllowedCapabilitiesJson { get; set; }

    /// <summary>
    /// Maksimum kullanıcı sayısı.
    /// 0 = sınırsız.
    /// </summary>
    public int MaxUsers { get; set; } = 3;

    /// <summary>
    /// Query history saklama süresi (gün).
    /// </summary>
    public int QueryHistoryRetentionDays { get; set; } = 30;

    /// <summary>
    /// Aylık fiyat (TL).
    /// 0 = ücretsiz.
    /// </summary>
    public decimal PriceMonthly { get; set; } = 0;

    /// <summary>
    /// Yıllık fiyat (TL) - indirimli.
    /// </summary>
    public decimal? PriceYearly { get; set; }

    /// <summary>
    /// Plan özellikleri (JSON array, marketing için).
    /// </summary>
    public string? FeaturesJson { get; set; }

    /// <summary>
    /// Plan aktif mi? (yeni müşterilere sunuluyor mu)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sıralama (UI gösterimi için).
    /// </summary>
    public int SortOrder { get; set; } = 0;

    // ===== Navigation Properties =====

    /// <summary>
    /// Bu planı kullanan tenant'lar.
    /// </summary>
    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();

    // ===== Helper Methods =====

    /// <summary>
    /// Allowed capabilities listesini döndürür.
    /// </summary>
    public List<string> GetAllowedCapabilities()
    {
        if (string.IsNullOrEmpty(AllowedCapabilitiesJson))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(AllowedCapabilitiesJson) 
            ?? new List<string>();
    }

    /// <summary>
    /// Features listesini döndürür.
    /// </summary>
    public List<string> GetFeatures()
    {
        if (string.IsNullOrEmpty(FeaturesJson))
            return new List<string>();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(FeaturesJson) 
            ?? new List<string>();
    }
}
