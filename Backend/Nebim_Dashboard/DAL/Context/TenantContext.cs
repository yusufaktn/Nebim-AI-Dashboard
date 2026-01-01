using Entity.Enums;

namespace DAL.Context;

/// <summary>
/// Mevcut request'in tenant bilgisini tutan context.
/// Scoped lifetime ile her request için ayrı instance.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Mevcut tenant ID.
    /// </summary>
    int? TenantId { get; set; }

    /// <summary>
    /// Mevcut user ID.
    /// </summary>
    int? UserId { get; set; }

    /// <summary>
    /// Tenant adı.
    /// </summary>
    string? TenantName { get; set; }

    /// <summary>
    /// Subscription tier.
    /// </summary>
    SubscriptionTier SubscriptionTier { get; set; }

    /// <summary>
    /// Tenant ID set edildi mi?
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Tenant ID'yi set et (middleware tarafından çağrılır).
    /// </summary>
    void SetTenant(int tenantId);

    /// <summary>
    /// Tenant context'i temizle.
    /// </summary>
    void Clear();
}

/// <summary>
/// ITenantContext implementasyonu.
/// </summary>
public class TenantContext : ITenantContext
{
    public int? TenantId { get; set; }
    public int? UserId { get; set; }
    public string? TenantName { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    public bool HasTenant => TenantId.HasValue;

    public void SetTenant(int tenantId)
    {
        TenantId = tenantId;
    }

    public void Clear()
    {
        TenantId = null;
        UserId = null;
        TenantName = null;
        SubscriptionTier = SubscriptionTier.Free;
    }
}
