using DAL.Providers;
using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;

namespace DAL.Repositories.Nebim;

/// <summary>
/// Tenant-aware simulation repository.
/// Merkezi MockDataProvider'dan verileri kullanır - tüm tenant'lar aynı verileri görür.
/// Development ve demo için kullanılır.
/// </summary>
public class SimulatedNebimRepository : INebimRepositoryWithContext
{
    private readonly string? _fallbackReason;

    // Merkezi veri kaynağından verileri al
    private IReadOnlyList<NebimProductDto> Products => MockDataProvider.Products;
    private IReadOnlyList<NebimStockDto> Stocks => MockDataProvider.Stocks;
    private IReadOnlyList<NebimSaleDto> Sales => MockDataProvider.Sales;
    private IReadOnlyList<NebimCustomerDto> Customers => MockDataProvider.Customers;

    public int? TenantId { get; }
    public bool IsSimulation { get; }
    public string DataSource => _fallbackReason != null 
        ? $"simulation (fallback: {_fallbackReason})" 
        : "simulation";

    public SimulatedNebimRepository(int? tenantId, bool isSimulation = true, string? fallbackReason = null)
    {
        TenantId = tenantId;
        IsSimulation = isSimulation;
        _fallbackReason = fallbackReason;

        // Artık tenant bazlı seed ile farklı veriler üretmiyoruz
        // Tüm veriler MockDataProvider'dan geliyor - tek veri kaynağı
    }

    #region Products

    public Task<PagedResult<NebimProductDto>> GetProductsAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        var query = Products.AsQueryable();

        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(p => p.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(filter.ProductName))
            query = query.Where(p => p.ProductName.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(filter.CategoryName))
            query = query.Where(p => p.CategoryName == filter.CategoryName);

        if (!string.IsNullOrEmpty(filter.BrandName))
            query = query.Where(p => p.BrandName == filter.BrandName);

        var totalCount = query.Count();
        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(PagedResult<NebimProductDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }

    public Task<NebimProductDto?> GetProductByCodeAsync(string productCode, CancellationToken ct = default)
    {
        var product = Products.FirstOrDefault(p => 
            p.ProductCode.Equals(productCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }

    public Task<List<NebimProductDto>> SearchProductsAsync(string searchTerm, int limit = 20, CancellationToken ct = default)
    {
        var results = Products
            .Where(p => p.ProductCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = Products
            .Select(p => p.CategoryName)
            .Where(c => c != null)
            .Cast<string>()
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        return Task.FromResult(categories);
    }

    public Task<List<string>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = Products
            .Select(p => p.BrandName)
            .Where(b => b != null)
            .Cast<string>()
            .Distinct()
            .OrderBy(b => b)
            .ToList();
        return Task.FromResult(brands);
    }

    #endregion

    #region Stocks

    public Task<PagedResult<NebimStockDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        var query = Stocks.AsQueryable();

        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(s => s.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(filter.WarehouseCode))
            query = query.Where(s => s.WarehouseCode == filter.WarehouseCode);

        if (filter.MinQuantity.HasValue)
            query = query.Where(s => s.Quantity >= filter.MinQuantity.Value);

        if (filter.InStockOnly == true)
            query = query.Where(s => s.Quantity > 0);

        var totalCount = query.Count();
        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(PagedResult<NebimStockDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }

    public Task<List<NebimStockDto>> GetStockByProductCodeAsync(string productCode, CancellationToken ct = default)
    {
        var stocks = Stocks
            .Where(s => s.ProductCode.Equals(productCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(stocks);
    }

    public Task<List<NebimStockDto>> GetLowStockItemsAsync(int threshold = 10, CancellationToken ct = default)
    {
        var lowStocks = Stocks
            .Where(s => s.Quantity > 0 && s.Quantity < threshold)
            .OrderBy(s => s.Quantity)
            .ToList();
        return Task.FromResult(lowStocks);
    }

    public Task<decimal> GetTotalStockValueAsync(CancellationToken ct = default)
    {
        var totalValue = Stocks.Sum(s => s.Quantity * 100m);
        return Task.FromResult(totalValue);
    }

    public Task<(int TotalRecords, int TotalQuantity)> GetStockSummaryAsync(CancellationToken ct = default)
    {
        var totalRecords = Stocks.Count;
        var totalQuantity = (int)Stocks.Sum(s => s.Quantity);
        return Task.FromResult((totalRecords, totalQuantity));
    }

    #endregion

    #region Sales

    public Task<PagedResult<NebimSaleDto>> GetSalesAsync(SalesFilterRequest filter, CancellationToken ct = default)
    {
        var query = Sales.AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(s => s.SaleDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(s => s.SaleDate <= filter.EndDate.Value);

        if (!string.IsNullOrEmpty(filter.StoreCode))
            query = query.Where(s => s.StoreCode == filter.StoreCode);

        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(s => s.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(s => s.SaleDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(PagedResult<NebimSaleDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }

    public Task<List<NebimSaleDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10, CancellationToken ct = default)
    {
        var topProducts = Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => s.ProductCode)
            .Select(g => new
            {
                ProductCode = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalAmount = g.Sum(x => x.TotalAmount),
                Sample = g.First()
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(limit)
            .Select(x => new NebimSaleDto
            {
                ProductCode = x.ProductCode,
                ProductName = x.Sample.ProductName,
                ColorName = x.Sample.ColorName,
                SizeName = x.Sample.SizeName,
                Quantity = x.TotalQuantity,
                TotalAmount = x.TotalAmount,
                SaleDate = endDate
            })
            .ToList();

        return Task.FromResult(topProducts);
    }

    public Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var total = Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .Sum(s => s.TotalAmount);
        return Task.FromResult(total);
    }

    public Task<Dictionary<DateTime, decimal>> GetDailySalesAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var dailySales = Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => s.SaleDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalAmount));
        return Task.FromResult(dailySales);
    }

    #endregion

    #region Customers

    public Task<PagedResult<NebimCustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        var query = Customers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.CustomerCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(PagedResult<NebimCustomerDto>.Create(items, page, pageSize, totalCount));
    }

    public Task<NebimCustomerDto?> GetCustomerByCodeAsync(string customerCode, CancellationToken ct = default)
    {
        var customer = Customers.FirstOrDefault(c =>
            c.CustomerCode.Equals(customerCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(customer);
    }

    public Task<List<NebimCustomerDto>> GetTopCustomersAsync(int limit = 10, CancellationToken ct = default)
    {
        var topCustomers = Customers
            .OrderByDescending(c => c.TotalSalesAmount)
            .Take(limit)
            .ToList();
        return Task.FromResult(topCustomers);
    }

    #endregion

    #region Dashboard

    public Task<int> GetTotalProductCountAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Products.Count);
    }

    public Task<int> GetTotalCustomerCountAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Customers.Count);
    }

    #endregion
}
