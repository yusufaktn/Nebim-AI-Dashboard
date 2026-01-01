namespace Entity.DTOs.Responses;

/// <summary>
/// Dashboard özet bilgileri response
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Günlük satış özeti
    /// </summary>
    public DailySalesSummary DailySales { get; set; } = new();
    
    /// <summary>
    /// Stok özeti
    /// </summary>
    public StockSummary StockSummary { get; set; } = new();
    
    /// <summary>
    /// En çok satan ürünler (Top 5)
    /// </summary>
    public List<TopProductDto> TopProducts { get; set; } = new();
    
    /// <summary>
    /// Mağaza bazlı satışlar
    /// </summary>
    public List<StoreSalesDto> StoreSales { get; set; } = new();
    
    /// <summary>
    /// Son 7 günlük satış trendi
    /// </summary>
    public List<DailySalesTrendDto> SalesTrend { get; set; } = new();
}

/// <summary>
/// Günlük satış özeti
/// </summary>
public class DailySalesSummary
{
    /// <summary>
    /// Bugünkü toplam satış tutarı
    /// </summary>
    public decimal TodayTotal { get; set; }
    
    /// <summary>
    /// Bugünkü satış adedi
    /// </summary>
    public int TodayCount { get; set; }
    
    /// <summary>
    /// Dünkü toplam satış tutarı
    /// </summary>
    public decimal YesterdayTotal { get; set; }
    
    /// <summary>
    /// Değişim yüzdesi
    /// </summary>
    public decimal ChangePercentage { get; set; }
    
    /// <summary>
    /// Aylık toplam satış
    /// </summary>
    public decimal MonthlyTotal { get; set; }
}

/// <summary>
/// Stok özeti
/// </summary>
public class StockSummary
{
    /// <summary>
    /// Toplam ürün sayısı
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Toplam stok miktarı
    /// </summary>
    public decimal TotalQuantity { get; set; }
    
    /// <summary>
    /// Toplam stok değeri
    /// </summary>
    public decimal TotalValue { get; set; }
    
    /// <summary>
    /// Kritik stok ürün sayısı (stok < 10)
    /// </summary>
    public int LowStockCount { get; set; }
    
    /// <summary>
    /// Stoksuz ürün sayısı
    /// </summary>
    public int OutOfStockCount { get; set; }
}

/// <summary>
/// En çok satan ürün
/// </summary>
public class TopProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Mağaza bazlı satış
/// </summary>
public class StoreSalesDto
{
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int SalesCount { get; set; }
}

/// <summary>
/// Günlük satış trendi
/// </summary>
public class DailySalesTrendDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int SalesCount { get; set; }
}
