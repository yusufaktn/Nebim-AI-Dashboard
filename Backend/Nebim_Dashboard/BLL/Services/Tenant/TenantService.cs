using DAL.Context;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Tenant;

/// <summary>
/// Tenant yönetim servisi implementasyonu.
/// </summary>
public class TenantService : ITenantService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        AppDbContext dbContext,
        ILogger<TenantService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TenantDto?> GetByIdAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        return tenant == null ? null : MapToDto(tenant);
    }

    public async Task<TenantDto?> GetByCompanyNameAsync(string companyName, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Name == companyName, ct);

        return tenant == null ? null : MapToDto(tenant);
    }

    public async Task<TenantDto> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        // Subscription plan kontrolü
        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId, ct);

        if (plan == null)
        {
            throw new InvalidOperationException($"Subscription plan bulunamadı: {request.SubscriptionPlanId}");
        }

        // Aynı firma adı kontrolü
        var existing = await _dbContext.Tenants
            .AnyAsync(t => t.Name == request.CompanyName, ct);

        if (existing)
        {
            throw new InvalidOperationException($"Bu firma adı zaten kullanılıyor: {request.CompanyName}");
        }

        // Slug oluştur
        var slug = GenerateSlug(request.CompanyName);

        var tenant = new Entity.App.Tenant
        {
            Name = request.CompanyName,
            Slug = slug,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            IsActive = true,
            OnboardingStatus = OnboardingStatus.NotStarted,
            ConnectionStatus = NebimConnectionStatus.NotConfigured,
            NebimServerType = NebimServerType.Simulation, // Varsayılan simulation
            SubscriptionPlanId = request.SubscriptionPlanId,
            SubscriptionStartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tenant created: {TenantId} - {Name}",
            tenant.Id, tenant.Name);

        return MapToDto(tenant);
    }

    public async Task<TenantDto> UpdateAsync(int tenantId, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı: {tenantId}");
        }

        if (!string.IsNullOrEmpty(request.CompanyName))
        {
            tenant.Name = request.CompanyName;
        }

        if (!string.IsNullOrEmpty(request.ContactEmail))
        {
            tenant.ContactEmail = request.ContactEmail;
        }

        if (request.ContactPhone != null)
        {
            tenant.ContactPhone = request.ContactPhone;
        }

        tenant.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Tenant updated: {TenantId}", tenantId);

        return MapToDto(tenant);
    }

    public async Task<bool> SetActiveStatusAsync(int tenantId, bool isActive, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);
        
        if (tenant == null)
        {
            return false;
        }

        tenant.IsActive = isActive;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tenant {TenantId} status changed to: {Status}",
            tenantId, isActive ? "Active" : "Inactive");

        return true;
    }

    public async Task<bool> UpdateSubscriptionAsync(
        int tenantId, 
        int subscriptionPlanId, 
        DateTime? endDate, 
        CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);
        
        if (tenant == null)
        {
            return false;
        }

        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == subscriptionPlanId, ct);

        if (plan == null)
        {
            throw new InvalidOperationException($"Subscription plan bulunamadı: {subscriptionPlanId}");
        }

        tenant.SubscriptionPlanId = subscriptionPlanId;
        tenant.SubscriptionStartDate = DateTime.UtcNow;
        tenant.SubscriptionEndDate = endDate;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tenant {TenantId} subscription updated to: {PlanName}",
            tenantId, plan.Name);

        return true;
    }

    private static string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }

    private static TenantDto MapToDto(Entity.App.Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            CompanyName = tenant.Name,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            IsActive = tenant.IsActive,
            OnboardingStatus = tenant.OnboardingStatus,
            NebimConnectionStatus = tenant.ConnectionStatus,
            UseSimulationMode = tenant.NebimServerType == NebimServerType.Simulation,
            SubscriptionTier = tenant.SubscriptionPlan?.Tier ?? SubscriptionTier.Free,
            SubscriptionEndDate = tenant.SubscriptionEndDate,
            CreatedAt = tenant.CreatedAt
        };
    }
}
