using DAL.Context;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Validation;

/// <summary>
/// Tenant doğrulayıcı implementasyonu.
/// </summary>
public class TenantValidator : ITenantValidator
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantConnectionManager _connectionManager;
    private readonly ILogger<TenantValidator> _logger;

    public TenantValidator(
        AppDbContext dbContext,
        ITenantConnectionManager connectionManager,
        ILogger<TenantValidator> logger)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<TenantValidationResult> ValidateAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            return TenantValidationResult.Failure("Tenant bulunamadı");
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is inactive", tenantId);
            return TenantValidationResult.Failure("Tenant aktif değil");
        }

        // Onboarding durumu kontrolü
        if (tenant.OnboardingStatus != OnboardingStatus.Completed)
        {
            _logger.LogWarning("Tenant {TenantId} onboarding incomplete: {Status}", tenantId, tenant.OnboardingStatus);
            return TenantValidationResult.Failure(
                $"Onboarding tamamlanmamış. Mevcut durum: {tenant.OnboardingStatus}");
        }

        // Subscription kontrolü
        if (tenant.SubscriptionPlan == null)
        {
            _logger.LogWarning("Tenant {TenantId} has no subscription plan", tenantId);
            return TenantValidationResult.Failure("Subscription planı bulunamadı");
        }

        if (tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Tenant {TenantId} subscription expired", tenantId);
            return TenantValidationResult.Failure(
                $"Subscription süresi doldu: {tenant.SubscriptionEndDate:yyyy-MM-dd}");
        }

        // Nebim bağlantı kontrolü (sadece Production mode için)
        var nebimConnected = true;
        var isSimulationMode = tenant.NebimServerType == NebimServerType.Simulation;
        
        if (tenant.ConnectionStatus == NebimConnectionStatus.Connected && !isSimulationMode)
        {
            // Gerçek bağlantı testi (optional, performans için skip edilebilir)
            // var testResult = await _connectionManager.TestConnectionAsync(tenantId, ct);
            // nebimConnected = testResult.IsSuccess;
        }
        else if (isSimulationMode)
        {
            nebimConnected = true; // Simulation modda her zaman bağlı
        }
        else if (tenant.ConnectionStatus != NebimConnectionStatus.Connected)
        {
            _logger.LogWarning("Tenant {TenantId} Nebim not connected: {Status}", 
                tenantId, tenant.ConnectionStatus);
            return TenantValidationResult.Failure(
                $"Nebim bağlantısı aktif değil: {tenant.ConnectionStatus}");
        }

        _logger.LogDebug(
            "Tenant {TenantId} validated. Tier: {Tier}, NebimConnected: {Connected}",
            tenantId, tenant.SubscriptionPlan.Tier, nebimConnected);

        return TenantValidationResult.Success(tenantId, tenant.SubscriptionPlan.Tier, nebimConnected);
    }
}
