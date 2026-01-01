using Entity.App;
using Entity.Enums;

namespace BLL.Services.Tenant;

/// <summary>
/// Tenant yönetim servisi interface.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Tenant bilgilerini getirir.
    /// </summary>
    Task<TenantDto?> GetByIdAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Firma adına göre tenant arar.
    /// </summary>
    Task<TenantDto?> GetByCompanyNameAsync(string companyName, CancellationToken ct = default);

    /// <summary>
    /// Yeni tenant oluşturur.
    /// </summary>
    Task<TenantDto> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Tenant bilgilerini günceller.
    /// </summary>
    Task<TenantDto> UpdateAsync(int tenantId, UpdateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Tenant'ı aktif/pasif yapar.
    /// </summary>
    Task<bool> SetActiveStatusAsync(int tenantId, bool isActive, CancellationToken ct = default);

    /// <summary>
    /// Tenant'ın subscription planını günceller.
    /// </summary>
    Task<bool> UpdateSubscriptionAsync(int tenantId, int subscriptionPlanId, DateTime? endDate, CancellationToken ct = default);
}

/// <summary>
/// Tenant DTO.
/// </summary>
public class TenantDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public OnboardingStatus OnboardingStatus { get; set; }
    public NebimConnectionStatus NebimConnectionStatus { get; set; }
    public bool UseSimulationMode { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Tenant oluşturma isteği.
/// </summary>
public class CreateTenantRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public int SubscriptionPlanId { get; set; }
}

/// <summary>
/// Tenant güncelleme isteği.
/// </summary>
public class UpdateTenantRequest
{
    public string? CompanyName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}
