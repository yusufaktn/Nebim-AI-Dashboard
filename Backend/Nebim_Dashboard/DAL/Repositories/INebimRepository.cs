using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;

namespace DAL.Repositories;

/// <summary>
/// Nebim V3 veritabanı için Repository Interface
/// Read-only işlemler (Dapper ile)
/// </summary>
public interface INebimRepository
{
    // Products
    Task<PagedResult<NebimProductDto>> GetProductsAsync(StockFilterRequest filter, CancellationToken ct = default);
    Task<NebimProductDto?> GetProductByCodeAsync(string productCode, CancellationToken ct = default);
    Task<List<NebimProductDto>> SearchProductsAsync(string searchTerm, int limit = 20, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<string>> GetBrandsAsync(CancellationToken ct = default);
    
    // Stocks
    Task<PagedResult<NebimStockDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken ct = default);
    Task<List<NebimStockDto>> GetStockByProductCodeAsync(string productCode, CancellationToken ct = default);
    Task<List<NebimStockDto>> GetLowStockItemsAsync(int threshold = 10, CancellationToken ct = default);
    Task<decimal> GetTotalStockValueAsync(CancellationToken ct = default);
    Task<(int TotalRecords, int TotalQuantity)> GetStockSummaryAsync(CancellationToken ct = default);
    
    // Sales
    Task<PagedResult<NebimSaleDto>> GetSalesAsync(SalesFilterRequest filter, CancellationToken ct = default);
    Task<List<NebimSaleDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10, CancellationToken ct = default);
    Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Dictionary<DateTime, decimal>> GetDailySalesAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    
    // Customers
    Task<PagedResult<NebimCustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default);
    Task<NebimCustomerDto?> GetCustomerByCodeAsync(string customerCode, CancellationToken ct = default);
    Task<List<NebimCustomerDto>> GetTopCustomersAsync(int limit = 10, CancellationToken ct = default);
    
    // Dashboard
    Task<int> GetTotalProductCountAsync(CancellationToken ct = default);
    Task<int> GetTotalCustomerCountAsync(CancellationToken ct = default);
}
