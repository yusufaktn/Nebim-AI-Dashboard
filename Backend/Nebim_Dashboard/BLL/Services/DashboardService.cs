using BLL.Helpers;
using BLL.Services.Interfaces;
using DAL.Repositories;
using Entity.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Dashboard servisi
/// 
/// 🎓 AÇIKLAMA:
/// - Ana sayfa için KPI ve özet verileri sağlar
/// - Nebim repository'den veri çeker ve hesaplamalar yapar
/// - Paralel çağrılar ile performans optimizasyonu
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly INebimRepository _nebimRepository;
    private readonly ILogger<DashboardService> _logger;
    
    public DashboardService(
        INebimRepository nebimRepository,
        ILogger<DashboardService> logger)
    {
        _nebimRepository = nebimRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Dashboard verilerini getir
    /// </summary>
    public async Task<DashboardResponse> GetDashboardDataAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Dashboard verileri getiriliyor");
        
        var today = DateHelper.TodayStart;
        var yesterday = today.AddDays(-1);
        var monthAgo = today.AddDays(-30); // 30 gün öncesi (AI ile tutarlı)
        var monthStart = DateHelper.MonthStart;
        
        // 🎓 Task.WhenAll: Paralel çağrılar
        // Bağımsız işlemleri aynı anda çalıştırarak toplam süreyi azaltır
        var todaySalesTask = _nebimRepository.GetTotalSalesAmountAsync(today, today.AddDays(1), ct);
        var yesterdaySalesTask = _nebimRepository.GetTotalSalesAmountAsync(yesterday, today, ct);
        var monthlySalesTask = _nebimRepository.GetTotalSalesAmountAsync(monthStart, today.AddDays(1), ct);
        var totalProductsTask = _nebimRepository.GetTotalProductCountAsync(ct);
        var lowStockTask = _nebimRepository.GetLowStockItemsAsync(10, ct);
        var topProductsTask = _nebimRepository.GetTopSellingProductsAsync(monthAgo, today.AddDays(1), 5, ct); // Son 30 gün
        var dailySalesTask = _nebimRepository.GetDailySalesAsync(monthAgo, today, ct); // Son 30 gün
        
        await Task.WhenAll(
            todaySalesTask, yesterdaySalesTask, monthlySalesTask,
            totalProductsTask, lowStockTask, topProductsTask, dailySalesTask);
        
        var todaySales = await todaySalesTask;
        var yesterdaySales = await yesterdaySalesTask;
        var monthlySales = await monthlySalesTask;
        var totalProducts = await totalProductsTask;
        var lowStockItems = await lowStockTask;
        var topProducts = await topProductsTask;
        var dailySales = await dailySalesTask;
        
        // 🎓 İş mantığı: Hesaplamalar BLL'de yapılır
        var changePercentage = DateHelper.CalculateChangePercentage(todaySales, yesterdaySales);
        
        var response = new DashboardResponse
        {
            DailySales = new DailySalesSummary
            {
                TodayTotal = todaySales,
                TodayCount = 0, // Mock'tan count gelmiyor, basit tutalım
                YesterdayTotal = yesterdaySales,
                ChangePercentage = changePercentage,
                MonthlyTotal = monthlySales
            },
            StockSummary = new StockSummary
            {
                TotalProducts = totalProducts,
                LowStockCount = lowStockItems.Count,
                OutOfStockCount = lowStockItems.Count(s => s.Quantity == 0)
            },
            TopProducts = topProducts.Select(p => new TopProductDto
            {
                ProductCode = p.ProductCode,
                ProductName = p.ProductName ?? "",
                TotalQuantity = (int)p.Quantity,
                TotalAmount = p.TotalAmount
            }).ToList(),
            SalesTrend = dailySales.Select(kv => new DailySalesTrendDto
            {
                Date = kv.Key,
                Amount = kv.Value
            }).OrderBy(d => d.Date).ToList()
        };
        
        _logger.LogInformation("Dashboard verileri hazırlandı. Günlük satış: {TodaySales:C}", todaySales);
        
        return response;
    }
    
    /// <summary>
    /// Düşük stok uyarıları
    /// </summary>
    public async Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 10, CancellationToken ct = default)
    {
        _logger.LogInformation("Düşük stok uyarıları getiriliyor. Eşik: {Threshold}", threshold);
        
        var lowStockItems = await _nebimRepository.GetLowStockItemsAsync(threshold, ct);
        
        return lowStockItems.Select(item => new LowStockAlertDto
        {
            ProductCode = item.ProductCode,
            ProductName = item.ProductName ?? "",
            WarehouseName = item.WarehouseName ?? "",
            CurrentQuantity = item.Quantity,
            Severity = item.Quantity <= 3 ? "Critical" : "Warning"
        }).ToList();
    }
}
