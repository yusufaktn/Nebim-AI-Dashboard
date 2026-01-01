using BLL.Services.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Tenant Onboarding API Controller.
/// Self-service Nebim bağlantı yapılandırması.
/// </summary>
[Authorize]
[Route("api/onboarding")]
public class TenantOnboardingController : BaseController
{
    private readonly ITenantOnboardingService _onboardingService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantOnboardingController> _logger;

    public TenantOnboardingController(
        ITenantOnboardingService onboardingService,
        ITenantService tenantService,
        ILogger<TenantOnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Onboarding durumunu getirir.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(OnboardingStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        var status = await _onboardingService.GetOnboardingStatusAsync(CurrentTenantId.Value, ct);
        return Success(status);
    }

    /// <summary>
    /// Nebim bağlantı bilgilerini yapılandırır.
    /// </summary>
    [HttpPost("nebim/configure")]
    [ProducesResponseType(typeof(NebimConnectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureNebim([FromBody] ConfigureNebimConnectionRequest request, CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        if (!IsTenantAdmin)
        {
            return Forbid();
        }

        // Validasyon
        if (string.IsNullOrWhiteSpace(request.ServerAddress))
        {
            return Error("Sunucu adresi gerekli");
        }

        if (string.IsNullOrWhiteSpace(request.DatabaseName))
        {
            return Error("Veritabanı adı gerekli");
        }

        _logger.LogInformation(
            "Configuring Nebim connection for tenant {TenantId}",
            CurrentTenantId);

        var result = await _onboardingService.ConfigureNebimConnectionAsync(CurrentTenantId.Value, request, ct);

        if (!result.Success)
        {
            return Error(result.Error ?? "Bağlantı yapılandırılamadı");
        }

        return Success(result);
    }

    /// <summary>
    /// Nebim bağlantısını test eder.
    /// </summary>
    [HttpPost("nebim/test")]
    [ProducesResponseType(typeof(NebimConnectionTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestNebimConnection(CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        _logger.LogInformation("Testing Nebim connection for tenant {TenantId}", CurrentTenantId);

        var result = await _onboardingService.TestNebimConnectionAsync(CurrentTenantId.Value, ct);
        return Success(result);
    }

    /// <summary>
    /// Simulation modunu etkinleştirir.
    /// </summary>
    [HttpPost("simulation/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnableSimulation(CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        if (!IsTenantAdmin)
        {
            return Forbid();
        }

        var result = await _onboardingService.EnableSimulationModeAsync(CurrentTenantId.Value, ct);
        return result ? Success("Simulation modu etkinleştirildi") : Error("İşlem başarısız");
    }

    /// <summary>
    /// Production modunu etkinleştirir.
    /// </summary>
    [HttpPost("production/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableProduction(CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        if (!IsTenantAdmin)
        {
            return Forbid();
        }

        try
        {
            var result = await _onboardingService.EnableProductionModeAsync(CurrentTenantId.Value, ct);
            return result 
                ? Success("Production modu etkinleştirildi") 
                : Error("İşlem başarısız");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Onboarding'i tamamlar.
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(CancellationToken ct)
    {
        if (!CurrentTenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        if (!IsTenantAdmin)
        {
            return Forbid();
        }

        try
        {
            var result = await _onboardingService.CompleteOnboardingAsync(CurrentTenantId.Value, ct);
            return result 
                ? Success("Onboarding tamamlandı") 
                : Error("İşlem başarısız");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }
}
