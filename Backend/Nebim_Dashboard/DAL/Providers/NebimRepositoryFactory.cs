using DAL.Context;
using DAL.Repositories.Nebim;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DAL.Providers;

/// <summary>
/// Tenant'a göre uygun INebimRepository döndüren factory.
/// Simulation mode için MockNebimRepository, Real mode için NebimRepository kullanır.
/// </summary>
public interface INebimRepositoryFactory
{
    /// <summary>
    /// Tenant'ın moduna göre uygun repository'yi oluşturur.
    /// </summary>
    Task<INebimRepositoryWithContext> CreateAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Simulation repository oluşturur (tenant bağımsız).
    /// </summary>
    INebimRepositoryWithContext CreateSimulation();
}

/// <summary>
/// Tenant context bilgisi içeren INebimRepository.
/// </summary>
public interface INebimRepositoryWithContext : DAL.Repositories.INebimRepository
{
    /// <summary>
    /// Repository'nin çalıştığı tenant ID.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Simulation modunda mı?
    /// </summary>
    bool IsSimulation { get; }

    /// <summary>
    /// Veri kaynağı adı.
    /// </summary>
    string DataSource { get; }
}

/// <summary>
/// INebimRepositoryFactory implementasyonu.
/// </summary>
public class NebimRepositoryFactory : INebimRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _context;
    private readonly ILogger<NebimRepositoryFactory> _logger;

    public NebimRepositoryFactory(
        IServiceProvider serviceProvider,
        AppDbContext context,
        ILogger<NebimRepositoryFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _logger = logger;
    }

    public async Task<INebimRepositoryWithContext> CreateAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant not found: {tenantId}");

        if (tenant.NebimServerType == NebimServerType.Simulation)
        {
            _logger.LogDebug("Creating simulation repository for tenant {TenantId}", tenantId);
            return new SimulatedNebimRepository(tenantId);
        }

        if (tenant.ConnectionStatus != NebimConnectionStatus.Connected)
        {
            _logger.LogWarning(
                "Tenant {TenantId} Nebim connection is not ready. Status: {Status}",
                tenantId, tenant.ConnectionStatus);
            
            // Bağlantı hazır değilse simulation'a fallback
            return new SimulatedNebimRepository(tenantId, isSimulation: true, fallbackReason: "Connection not ready");
        }

        _logger.LogDebug("Creating real Nebim repository for tenant {TenantId}", tenantId);
        
        var connectionManager = _serviceProvider.GetRequiredService<ITenantConnectionManager>();
        return new TenantAwareNebimRepository(tenantId, connectionManager, _logger);
    }

    public INebimRepositoryWithContext CreateSimulation()
    {
        return new SimulatedNebimRepository(null);
    }
}
