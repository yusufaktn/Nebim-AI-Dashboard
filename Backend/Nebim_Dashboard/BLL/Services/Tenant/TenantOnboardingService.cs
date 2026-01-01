using DAL.Context;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Tenant;

/// <summary>
/// Tenant onboarding servisi implementasyonu.
/// </summary>
public class TenantOnboardingService : ITenantOnboardingService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantConnectionManager _connectionManager;
    private readonly ILogger<TenantOnboardingService> _logger;

    public TenantOnboardingService(
        AppDbContext dbContext,
        ITenantConnectionManager connectionManager,
        ILogger<TenantOnboardingService> logger)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<OnboardingStatusDto> GetOnboardingStatusAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı: {tenantId}");
        }

        var pendingSteps = new List<string>();
        var isSimulationMode = tenant.NebimServerType == NebimServerType.Simulation;

        if (tenant.OnboardingStatus == OnboardingStatus.PendingNebimConnection)
        {
            pendingSteps.Add("Nebim bağlantısını yapılandırın veya simulation modunu etkinleştirin");
        }

        if (tenant.OnboardingStatus == OnboardingStatus.TestingConnection &&
            tenant.ConnectionStatus != NebimConnectionStatus.Connected)
        {
            pendingSteps.Add("Nebim bağlantısını test edin");
        }

        var canComplete = isSimulationMode ||
                         tenant.ConnectionStatus == NebimConnectionStatus.Connected;

        return new OnboardingStatusDto
        {
            Status = tenant.OnboardingStatus,
            NebimStatus = tenant.ConnectionStatus,
            UseSimulationMode = isSimulationMode,
            CanComplete = canComplete,
            PendingSteps = pendingSteps,
            LastConnectionTest = tenant.NebimLastConnectedAt,
            LastConnectionError = tenant.NebimLastErrorMessage
        };
    }

    public async Task<NebimConnectionResult> ConfigureNebimConnectionAsync(
        int tenantId,
        ConfigureNebimConnectionRequest request,
        CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            return NebimConnectionResult.Failed("Tenant bulunamadı");
        }

        try
        {
            // Connection string oluştur
            var connectionString = BuildConnectionString(request);

            // Şifrele ve kaydet
            tenant.NebimServerHost = request.ServerAddress;
            tenant.NebimDatabaseName = request.DatabaseName;
            tenant.NebimServerType = NebimServerType.Real;
            tenant.NebimConnectionStringEncrypted = _connectionManager.EncryptConnectionString(connectionString);
            tenant.ConnectionStatus = NebimConnectionStatus.Pending;
            tenant.OnboardingStatus = OnboardingStatus.TestingConnection;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Nebim connection configured for tenant {TenantId}",
                tenantId);

            return NebimConnectionResult.Succeeded(NebimConnectionStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Nebim connection for tenant {TenantId}", tenantId);
            return NebimConnectionResult.Failed(ex.Message);
        }
    }

    public async Task<NebimConnectionTestResult> TestNebimConnectionAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            return NebimConnectionTestResult.Failed("Tenant bulunamadı");
        }

        var isSimulationMode = tenant.NebimServerType == NebimServerType.Simulation;

        if (isSimulationMode)
        {
            return NebimConnectionTestResult.Succeeded(0, "Simulation Mode");
        }

        if (string.IsNullOrEmpty(tenant.NebimConnectionStringEncrypted))
        {
            return NebimConnectionTestResult.Failed("Nebim bağlantısı yapılandırılmamış");
        }

        try
        {
            var testResult = await _connectionManager.TestConnectionAsync(tenantId, ct);

            tenant.NebimLastConnectedAt = DateTime.UtcNow;

            if (testResult.IsSuccess)
            {
                tenant.ConnectionStatus = NebimConnectionStatus.Connected;
                tenant.NebimLastErrorMessage = null;
                tenant.OnboardingStatus = OnboardingStatus.Completed;
            }
            else
            {
                tenant.ConnectionStatus = NebimConnectionStatus.Failed;
                tenant.NebimLastErrorMessage = testResult.ErrorMessage;
            }

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Nebim connection test for tenant {TenantId}: {Result}",
                tenantId, testResult.IsSuccess ? "Success" : "Failed");

            return testResult.IsSuccess
                ? NebimConnectionTestResult.Succeeded(testResult.ResponseTimeMs, testResult.ServerVersion)
                : NebimConnectionTestResult.Failed(testResult.ErrorMessage ?? "Bağlantı testi başarısız");
        }
        catch (Exception ex)
        {
            tenant.ConnectionStatus = NebimConnectionStatus.Failed;
            tenant.NebimLastConnectedAt = DateTime.UtcNow;
            tenant.NebimLastErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogError(ex, "Nebim connection test failed for tenant {TenantId}", tenantId);
            return NebimConnectionTestResult.Failed(ex.Message);
        }
    }

    public async Task<bool> EnableSimulationModeAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            return false;
        }

        tenant.NebimServerType = NebimServerType.Simulation;
        tenant.OnboardingStatus = OnboardingStatus.Completed; // Simulation hazır
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Simulation mode enabled for tenant {TenantId}", tenantId);
        return true;
    }

    public async Task<bool> EnableProductionModeAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            return false;
        }

        // Production mode için Nebim bağlantısı gerekli
        if (tenant.ConnectionStatus != NebimConnectionStatus.Connected)
        {
            throw new InvalidOperationException(
                "Production moduna geçmek için Nebim bağlantısı gerekli");
        }

        tenant.NebimServerType = NebimServerType.Real;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Production mode enabled for tenant {TenantId}", tenantId);
        return true;
    }

    public async Task<bool> CompleteOnboardingAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FindAsync(new object[] { tenantId }, ct);

        if (tenant == null)
        {
            return false;
        }

        var isSimulationMode = tenant.NebimServerType == NebimServerType.Simulation;

        // Tamamlama kontrolü
        var canComplete = isSimulationMode ||
                         tenant.ConnectionStatus == NebimConnectionStatus.Connected;

        if (!canComplete)
        {
            throw new InvalidOperationException(
                "Onboarding tamamlanamaz. Nebim bağlantısı veya simulation modu gerekli.");
        }

        tenant.OnboardingStatus = OnboardingStatus.Completed;
        tenant.OnboardingCompletedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Onboarding completed for tenant {TenantId}", tenantId);
        return true;
    }

    private static string BuildConnectionString(ConfigureNebimConnectionRequest request)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = request.Port.HasValue 
                ? $"{request.ServerAddress},{request.Port.Value}"
                : request.ServerAddress,
            InitialCatalog = request.DatabaseName,
            IntegratedSecurity = request.UseIntegratedSecurity
        };

        if (!request.UseIntegratedSecurity)
        {
            builder.UserID = request.Username;
            builder.Password = request.Password;
        }

        return builder.ConnectionString;
    }
}
