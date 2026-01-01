using BLL.Services.Interfaces;
using DAL.Repositories;
using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Stok servisi implementasyonu
/// 
/// 🎓 AÇIKLAMA:
/// - Bu servis Nebim veritabanından stok verilerini çeker
/// - INebimRepository üzerinden çalışır (şu an Mock, sonra gerçek Dapper)
/// - Servis katmanında iş mantığı uygulanabilir (filtreleme, hesaplama vb.)
/// </summary>
public class StockService : IStockService
{
    private readonly INebimRepository _nebimRepository;
    private readonly ILogger<StockService> _logger;
    
    // 🎓 Constructor Injection:
    // - Bağımlılıklar dışarıdan verilir (DI Container tarafından)
    // - Test edilebilirlik: Mock repository verilebilir
    // - Loose coupling: Somut sınıfa değil, interface'e bağımlı
    public StockService(
        INebimRepository nebimRepository,
        ILogger<StockService> logger)
    {
        _nebimRepository = nebimRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Stok listesi (sayfalı)
    /// </summary>
    public async Task<PagedResult<NebimStockDto>> GetStocksAsync(
        StockFilterRequest filter, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Stok listesi getiriliyor. Sayfa: {Page}, Filtre: {@Filter}", 
            filter.Page, filter);
        
        var result = await _nebimRepository.GetStocksAsync(filter, ct);
        
        _logger.LogInformation("Stok listesi getirildi. Toplam: {Total}", result.TotalCount);
        
        return result;
    }
    
    /// <summary>
    /// Ürün detayı
    /// </summary>
    public async Task<NebimProductDto?> GetProductAsync(string productCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            _logger.LogWarning("Geçersiz ürün kodu");
            return null;
        }
        
        _logger.LogInformation("Ürün detayı getiriliyor: {ProductCode}", productCode);
        
        return await _nebimRepository.GetProductByCodeAsync(productCode, ct);
    }
    
    /// <summary>
    /// Ürün ara
    /// </summary>
    public async Task<List<NebimProductDto>> SearchProductsAsync(
        string searchTerm, 
        int limit = 20, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            _logger.LogWarning("Arama terimi çok kısa: {SearchTerm}", searchTerm);
            return new List<NebimProductDto>();
        }
        
        _logger.LogInformation("Ürün aranıyor: {SearchTerm}", searchTerm);
        
        return await _nebimRepository.SearchProductsAsync(searchTerm, limit, ct);
    }
    
    /// <summary>
    /// Kategori listesi
    /// </summary>
    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Kategori listesi getiriliyor");
        return await _nebimRepository.GetCategoriesAsync(ct);
    }
    
    /// <summary>
    /// Marka listesi
    /// </summary>
    public async Task<List<string>> GetBrandsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Marka listesi getiriliyor");
        return await _nebimRepository.GetBrandsAsync(ct);
    }
}
