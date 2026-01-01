using Api.Common;
using BLL.Services.Interfaces;
using Entity.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Dashboard controller'ı
/// 
/// 🎓 [Authorize]: Sadece giriş yapmış kullanıcılar erişebilir
/// </summary>
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Dashboard ana verilerini getir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard(CancellationToken ct)
    {
        var data = await _dashboardService.GetDashboardDataAsync(ct);
        return Ok(ApiResponse<DashboardResponse>.Success(data));
    }

    /// <summary>
    /// Düşük stok uyarıları
    /// </summary>
    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(typeof(ApiResponse<List<LowStockAlertDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LowStockAlertDto>>>> GetLowStockAlerts(
        [FromQuery] int threshold = 10,
        CancellationToken ct = default)
    {
        var alerts = await _dashboardService.GetLowStockAlertsAsync(threshold, ct);
        return Ok(ApiResponse<List<LowStockAlertDto>>.Success(alerts));
    }
}
