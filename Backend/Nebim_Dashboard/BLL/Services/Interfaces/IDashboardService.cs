using Entity.DTOs.Responses;

namespace BLL.Services.Interfaces;

/// <summary>
/// Dashboard servisi
/// Ana sayfa KPI ve grafik verileri
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Dashboard verilerini getir
    /// </summary>
    Task<DashboardResponse> GetDashboardDataAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Düşük stok uyarıları
    /// </summary>
    Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10, CancellationToken ct = default);
}
