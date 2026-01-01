using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;

namespace BLL.Services.Interfaces;

/// <summary>
/// Stok servisi
/// Nebim'den stok verilerini çeker
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Stok listesi (sayfalı)
    /// </summary>
    Task<PagedResult<NebimStockDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken ct = default);
    
    /// <summary>
    /// Ürün detayı
    /// </summary>
    Task<NebimProductDto?> GetProductAsync(string productCode, CancellationToken ct = default);
    
    /// <summary>
    /// Ürün ara
    /// </summary>
    Task<List<NebimProductDto>> SearchProductsAsync(string searchTerm, int limit = 20, CancellationToken ct = default);
    
    /// <summary>
    /// Kategori listesi
    /// </summary>
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Marka listesi
    /// </summary>
    Task<List<string>> GetBrandsAsync(CancellationToken ct = default);
}
