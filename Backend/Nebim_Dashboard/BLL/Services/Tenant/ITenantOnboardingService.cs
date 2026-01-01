using Entity.Enums;

namespace BLL.Services.Tenant;

/// <summary>
/// Tenant onboarding servisi interface.
/// Self-service Nebim bağlantısı yapılandırma.
/// </summary>
public interface ITenantOnboardingService
{
    /// <summary>
    /// Tenant'ın onboarding durumunu getirir.
    /// </summary>
    Task<OnboardingStatusDto> GetOnboardingStatusAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Nebim bağlantı bilgilerini yapılandırır.
    /// </summary>
    Task<NebimConnectionResult> ConfigureNebimConnectionAsync(
        int tenantId, 
        ConfigureNebimConnectionRequest request, 
        CancellationToken ct = default);

    /// <summary>
    /// Nebim bağlantısını test eder.
    /// </summary>
    Task<NebimConnectionTestResult> TestNebimConnectionAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Simulation moduna geçer.
    /// </summary>
    Task<bool> EnableSimulationModeAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Production moduna geçer (Nebim bağlantısı gerekli).
    /// </summary>
    Task<bool> EnableProductionModeAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Onboarding'i tamamlar.
    /// </summary>
    Task<bool> CompleteOnboardingAsync(int tenantId, CancellationToken ct = default);
}

/// <summary>
/// Onboarding durumu DTO.
/// </summary>
public class OnboardingStatusDto
{
    public OnboardingStatus Status { get; set; }
    public NebimConnectionStatus NebimStatus { get; set; }
    public bool UseSimulationMode { get; set; }
    public bool CanComplete { get; set; }
    public List<string> PendingSteps { get; set; } = new();
    public DateTime? LastConnectionTest { get; set; }
    public string? LastConnectionError { get; set; }
}

/// <summary>
/// Nebim bağlantı yapılandırma isteği.
/// </summary>
public class ConfigureNebimConnectionRequest
{
    public string ServerAddress { get; set; } = string.Empty;
    public int? Port { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseIntegratedSecurity { get; set; }
    public string? AdditionalOptions { get; set; }
}

/// <summary>
/// Nebim bağlantı yapılandırma sonucu.
/// </summary>
public class NebimConnectionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public NebimConnectionStatus NewStatus { get; set; }

    public static NebimConnectionResult Succeeded(NebimConnectionStatus status) => new()
    {
        Success = true,
        NewStatus = status
    };

    public static NebimConnectionResult Failed(string error) => new()
    {
        Success = false,
        Error = error,
        NewStatus = NebimConnectionStatus.Failed
    };
}

/// <summary>
/// Nebim bağlantı test sonucu.
/// </summary>
public class NebimConnectionTestResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public long LatencyMs { get; set; }
    public string? ServerVersion { get; set; }
    public string? DatabaseInfo { get; set; }
    public DateTime TestedAt { get; set; }

    public static NebimConnectionTestResult Succeeded(long latencyMs, string? serverVersion = null) => new()
    {
        Success = true,
        LatencyMs = latencyMs,
        ServerVersion = serverVersion,
        TestedAt = DateTime.UtcNow
    };

    public static NebimConnectionTestResult Failed(string error) => new()
    {
        Success = false,
        Error = error,
        TestedAt = DateTime.UtcNow
    };
}
