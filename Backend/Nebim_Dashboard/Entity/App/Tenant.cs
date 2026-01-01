using Entity.Base;
using Entity.Enums;

namespace Entity.App;

/// <summary>
/// Firma/Kiracı entity'si.
/// Her tenant kendi Nebim instance'ına bağlanır.
/// </summary>
public class Tenant : AuditableEntity
{
    /// <summary>
    /// Firma adı.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// URL-friendly unique tanımlayıcı.
    /// Örnek: "abc-tekstil", "xyz-perakende"
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Vergi numarası (opsiyonel).
    /// </summary>
    public string? TaxNumber { get; set; }

    /// <summary>
    /// İletişim e-postası.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// İletişim telefonu.
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Firma adresi.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Tenant aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ===== Nebim Bağlantı Bilgileri =====

    /// <summary>
    /// Nebim veri kaynağı türü.
    /// </summary>
    public NebimServerType NebimServerType { get; set; } = NebimServerType.Simulation;

    /// <summary>
    /// Şifrelenmiş Nebim connection string.
    /// AES-256 ile şifrelenir.
    /// </summary>
    public string? NebimConnectionStringEncrypted { get; set; }

    /// <summary>
    /// Nebim sunucu adresi (bilgi amaçlı).
    /// Connection string'den parse edilir.
    /// </summary>
    public string? NebimServerHost { get; set; }

    /// <summary>
    /// Nebim veritabanı adı (bilgi amaçlı).
    /// </summary>
    public string? NebimDatabaseName { get; set; }

    /// <summary>
    /// Nebim bağlantı durumu.
    /// </summary>
    public NebimConnectionStatus ConnectionStatus { get; set; } = NebimConnectionStatus.NotConfigured;

    /// <summary>
    /// Son başarılı bağlantı zamanı.
    /// </summary>
    public DateTime? NebimLastConnectedAt { get; set; }

    /// <summary>
    /// Son bağlantı hatası mesajı.
    /// </summary>
    public string? NebimLastErrorMessage { get; set; }

    /// <summary>
    /// Bağlantı test sayısı (retry tracking).
    /// </summary>
    public int ConnectionTestCount { get; set; } = 0;

    // ===== Onboarding =====

    /// <summary>
    /// Onboarding durumu.
    /// </summary>
    public OnboardingStatus OnboardingStatus { get; set; } = OnboardingStatus.NotStarted;

    /// <summary>
    /// Onboarding tamamlanma tarihi.
    /// </summary>
    public DateTime? OnboardingCompletedAt { get; set; }

    // ===== Subscription =====

    /// <summary>
    /// Subscription plan ID.
    /// </summary>
    public int SubscriptionPlanId { get; set; }

    /// <summary>
    /// Subscription başlangıç tarihi.
    /// </summary>
    public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Subscription bitiş tarihi (null = süresiz).
    /// </summary>
    public DateTime? SubscriptionEndDate { get; set; }

    // ===== Navigation Properties =====

    /// <summary>
    /// Subscription plan.
    /// </summary>
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    /// <summary>
    /// Tenant'a ait kullanıcılar.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// Tenant'ın query geçmişi.
    /// </summary>
    public virtual ICollection<QueryHistory> QueryHistories { get; set; } = new List<QueryHistory>();

    /// <summary>
    /// Tenant'ın quota kayıtları.
    /// </summary>
    public virtual ICollection<QueryQuota> QueryQuotas { get; set; } = new List<QueryQuota>();
}
