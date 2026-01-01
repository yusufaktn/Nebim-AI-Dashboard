using Entity.DTOs.Common;

namespace Entity.DTOs.Requests;

/// <summary>
/// Stok listesi filtreleme request
/// </summary>
public class StockFilterRequest : PagedRequest
{
    /// <summary>
    /// Ürün kodu ile ara
    /// </summary>
    public string? ProductCode { get; set; }
    
    /// <summary>
    /// Ürün adı ile ara
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Kategori filtresi
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// Marka filtresi
    /// </summary>
    public string? BrandName { get; set; }
    
    /// <summary>
    /// Depo kodu filtresi
    /// </summary>
    public string? WarehouseCode { get; set; }
    
    /// <summary>
    /// Minimum stok miktarı
    /// </summary>
    public decimal? MinQuantity { get; set; }
    
    /// <summary>
    /// Maksimum stok miktarı
    /// </summary>
    public decimal? MaxQuantity { get; set; }
    
    /// <summary>
    /// Sadece stokta olanları getir
    /// </summary>
    public bool? InStockOnly { get; set; }
    
    /// <summary>
    /// Sadece kritik stokları getir (stok < 10)
    /// </summary>
    public bool? LowStockOnly { get; set; }
}
