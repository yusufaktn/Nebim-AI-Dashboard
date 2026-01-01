using BLL.Services.AI;
using Entity.DTOs.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// İş Zekası API Controller.
/// AI destekli doğal dil sorgularını işler.
/// </summary>
[Authorize]
[Route("api/bi")]
public class BusinessIntelligenceController : BaseController
{
    private readonly IBusinessIntelligenceService _biService;
    private readonly ILogger<BusinessIntelligenceController> _logger;

    public BusinessIntelligenceController(
        IBusinessIntelligenceService biService,
        ILogger<BusinessIntelligenceController> logger)
    {
        _biService = biService;
        _logger = logger;
    }

    /// <summary>
    /// Doğal dil sorgusu işler.
    /// </summary>
    /// <remarks>
    /// Örnek sorgular:
    /// - "Bu ayki satışları göster"
    /// - "En çok satan 5 ürün"
    /// - "Düşük stoklu ürünler"
    /// - "Geçen ay ile bu ayı karşılaştır"
    /// </remarks>
    [HttpPost("query")]
    [ProducesResponseType(typeof(BusinessQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Query([FromBody] BusinessQueryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Error("Sorgu boş olamaz");
        }

        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (!tenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        if (!userId.HasValue)
        {
            return Error("Kullanıcı bilgisi bulunamadı", 401);
        }

        _logger.LogInformation(
            "BI Query from tenant {TenantId}, user {UserId}: {Query}",
            tenantId, userId, request.Query);

        var result = await _biService.ProcessQueryAsync(request, tenantId.Value, userId.Value, ct);

        if (!result.IsSuccess)
        {
            // Kota aşımı
            if (result.ErrorCode == "QUOTA_EXCEEDED")
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, result);
            }

            // Validation hataları
            if (result.ErrorCode == "VALIDATION_FAILED" || result.ErrorCode == "OUT_OF_SCOPE")
            {
                return BadRequest(result);
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Mevcut capability'leri listeler.
    /// </summary>
    [HttpGet("capabilities")]
    [ProducesResponseType(typeof(List<CapabilityInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCapabilities(CancellationToken ct)
    {
        var tenantId = GetTenantId();

        if (!tenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        var capabilities = await _biService.GetAvailableCapabilitiesAsync(tenantId.Value, ct);
        return Success(capabilities);
    }

    /// <summary>
    /// Sorgu geçmişini döndürür.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<QueryHistorySummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (!tenantId.HasValue)
        {
            return Error("Tenant bilgisi bulunamadı");
        }

        var history = await _biService.GetQueryHistoryAsync(tenantId.Value, userId, limit, ct);
        return Success(history);
    }

    #region Helper Methods

    private int? GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst("user_id") ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    #endregion
}
