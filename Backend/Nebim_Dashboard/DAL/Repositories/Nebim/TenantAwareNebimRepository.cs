using System.Data;
using Dapper;
using DAL.Context;
using DAL.Providers;
using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories.Nebim;

/// <summary>
/// Gerçek Nebim V3 veritabanına bağlanan tenant-aware repository.
/// </summary>
public class TenantAwareNebimRepository : INebimRepositoryWithContext
{
    private readonly int _tenantId;
    private readonly ITenantConnectionManager _connectionManager;
    private readonly ILogger _logger;

    public int? TenantId => _tenantId;
    public bool IsSimulation => false;
    public string DataSource => "real";

    public TenantAwareNebimRepository(
        int tenantId,
        ITenantConnectionManager connectionManager,
        ILogger logger)
    {
        _tenantId = tenantId;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    private async Task<IDbConnection> GetConnectionAsync(CancellationToken ct)
    {
        return await _connectionManager.GetNebimConnectionAsync(_tenantId, ct);
    }

    #region Products

    public async Task<PagedResult<NebimProductDto>> GetProductsAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                p.ItemCode as ProductCode,
                p.ItemDescription as ProductName,
                p.ItemCategoryCode as CategoryName,
                p.BrandCode as BrandName,
                p.ColorCode as ColorName,
                p.UnitPrice,
                p.Barcode,
                p.IsBlocked as IsActive
            FROM cdItem p
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(filter.ProductCode))
        {
            sql += " AND p.ItemCode LIKE @ProductCode";
            parameters.Add("ProductCode", $"%{filter.ProductCode}%");
        }

        if (!string.IsNullOrEmpty(filter.ProductName))
        {
            sql += " AND p.ItemDescription LIKE @ProductName";
            parameters.Add("ProductName", $"%{filter.ProductName}%");
        }

        if (!string.IsNullOrEmpty(filter.CategoryName))
        {
            sql += " AND p.ItemCategoryCode = @CategoryName";
            parameters.Add("CategoryName", filter.CategoryName);
        }

        if (!string.IsNullOrEmpty(filter.BrandName))
        {
            sql += " AND p.BrandCode = @BrandName";
            parameters.Add("BrandName", filter.BrandName);
        }

        // Count query
        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Paginated query
        sql += @" ORDER BY p.ItemCode
                  OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
        parameters.Add("PageSize", filter.PageSize);

        var items = (await connection.QueryAsync<NebimProductDto>(sql, parameters)).ToList();

        return PagedResult<NebimProductDto>.Create(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<NebimProductDto?> GetProductByCodeAsync(string productCode, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT TOP 1
                p.ItemCode as ProductCode,
                p.ItemDescription as ProductName,
                p.ItemCategoryCode as CategoryName,
                p.BrandCode as BrandName,
                p.ColorCode as ColorName,
                p.UnitPrice,
                p.Barcode,
                p.IsBlocked as IsActive
            FROM cdItem p
            WHERE p.ItemCode = @ProductCode";

        return await connection.QueryFirstOrDefaultAsync<NebimProductDto>(sql, new { ProductCode = productCode });
    }

    public async Task<List<NebimProductDto>> SearchProductsAsync(string searchTerm, int limit = 20, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT TOP (@Limit)
                p.ItemCode as ProductCode,
                p.ItemDescription as ProductName,
                p.ItemCategoryCode as CategoryName,
                p.BrandCode as BrandName,
                p.ColorCode as ColorName,
                p.UnitPrice,
                p.Barcode,
                p.IsBlocked as IsActive
            FROM cdItem p
            WHERE p.ItemCode LIKE @SearchTerm OR p.ItemDescription LIKE @SearchTerm
            ORDER BY p.ItemCode";

        var results = await connection.QueryAsync<NebimProductDto>(sql, new { Limit = limit, SearchTerm = $"%{searchTerm}%" });
        return results.ToList();
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = "SELECT DISTINCT ItemCategoryCode FROM cdItem WHERE ItemCategoryCode IS NOT NULL ORDER BY ItemCategoryCode";
        var results = await connection.QueryAsync<string>(sql);
        return results.ToList();
    }

    public async Task<List<string>> GetBrandsAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = "SELECT DISTINCT BrandCode FROM cdItem WHERE BrandCode IS NOT NULL ORDER BY BrandCode";
        var results = await connection.QueryAsync<string>(sql);
        return results.ToList();
    }

    #endregion

    #region Stocks

    public async Task<PagedResult<NebimStockDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                s.ItemCode as ProductCode,
                i.ItemDescription as ProductName,
                s.WarehouseCode,
                w.WarehouseDescription as WarehouseName,
                s.Qty as Quantity,
                s.ReservedQty as ReservedQuantity,
                (s.Qty - s.ReservedQty) as AvailableQuantity
            FROM prInventory s
            INNER JOIN cdItem i ON s.ItemCode = i.ItemCode
            LEFT JOIN cdWarehouse w ON s.WarehouseCode = w.WarehouseCode
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(filter.ProductCode))
        {
            sql += " AND s.ItemCode LIKE @ProductCode";
            parameters.Add("ProductCode", $"%{filter.ProductCode}%");
        }

        if (!string.IsNullOrEmpty(filter.WarehouseCode))
        {
            sql += " AND s.WarehouseCode = @WarehouseCode";
            parameters.Add("WarehouseCode", filter.WarehouseCode);
        }

        if (filter.MinQuantity.HasValue)
        {
            sql += " AND s.Qty >= @MinQuantity";
            parameters.Add("MinQuantity", filter.MinQuantity.Value);
        }

        if (filter.InStockOnly == true)
        {
            sql += " AND s.Qty > 0";
        }

        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        sql += @" ORDER BY s.ItemCode
                  OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
        parameters.Add("PageSize", filter.PageSize);

        var items = (await connection.QueryAsync<NebimStockDto>(sql, parameters)).ToList();

        return PagedResult<NebimStockDto>.Create(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<List<NebimStockDto>> GetStockByProductCodeAsync(string productCode, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                s.ItemCode as ProductCode,
                i.ItemDescription as ProductName,
                s.WarehouseCode,
                w.WarehouseDescription as WarehouseName,
                s.Qty as Quantity,
                s.ReservedQty as ReservedQuantity,
                (s.Qty - s.ReservedQty) as AvailableQuantity
            FROM prInventory s
            INNER JOIN cdItem i ON s.ItemCode = i.ItemCode
            LEFT JOIN cdWarehouse w ON s.WarehouseCode = w.WarehouseCode
            WHERE s.ItemCode = @ProductCode";

        var results = await connection.QueryAsync<NebimStockDto>(sql, new { ProductCode = productCode });
        return results.ToList();
    }

    public async Task<List<NebimStockDto>> GetLowStockItemsAsync(int threshold = 10, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                s.ItemCode as ProductCode,
                i.ItemDescription as ProductName,
                s.WarehouseCode,
                w.WarehouseDescription as WarehouseName,
                s.Qty as Quantity,
                s.ReservedQty as ReservedQuantity,
                (s.Qty - s.ReservedQty) as AvailableQuantity
            FROM prInventory s
            INNER JOIN cdItem i ON s.ItemCode = i.ItemCode
            LEFT JOIN cdWarehouse w ON s.WarehouseCode = w.WarehouseCode
            WHERE s.Qty > 0 AND s.Qty < @Threshold
            ORDER BY s.Qty";

        var results = await connection.QueryAsync<NebimStockDto>(sql, new { Threshold = threshold });
        return results.ToList();
    }

    public async Task<decimal> GetTotalStockValueAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT ISNULL(SUM(s.Qty * i.UnitPrice), 0)
            FROM prInventory s
            INNER JOIN cdItem i ON s.ItemCode = i.ItemCode";

        return await connection.ExecuteScalarAsync<decimal>(sql);
    }

    public async Task<(int TotalRecords, int TotalQuantity)> GetStockSummaryAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                COUNT(*) as TotalRecords,
                ISNULL(SUM(CAST(s.Qty AS INT)), 0) as TotalQuantity
            FROM prInventory s
            WHERE s.Qty > 0";

        var result = await connection.QueryFirstAsync<(int TotalRecords, int TotalQuantity)>(sql);
        return result;
    }

    #endregion

    #region Sales

    public async Task<PagedResult<NebimSaleDto>> GetSalesAsync(SalesFilterRequest filter, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                t.TransactionID as SaleId,
                tl.ItemCode as ProductCode,
                i.ItemDescription as ProductName,
                t.OfficeCode as StoreCode,
                o.OfficeName as StoreName,
                tl.Qty as Quantity,
                tl.UnitPrice,
                tl.DiscountAmount as Discount,
                tl.NetAmount as TotalAmount,
                t.TransactionDate as SaleDate,
                t.CurrAccCode as CustomerCode
            FROM trSalesTransaction t
            INNER JOIN trSalesTransactionLine tl ON t.TransactionID = tl.TransactionID
            INNER JOIN cdItem i ON tl.ItemCode = i.ItemCode
            LEFT JOIN cdOffice o ON t.OfficeCode = o.OfficeCode
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (filter.StartDate.HasValue)
        {
            sql += " AND t.TransactionDate >= @StartDate";
            parameters.Add("StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            sql += " AND t.TransactionDate <= @EndDate";
            parameters.Add("EndDate", filter.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.StoreCode))
        {
            sql += " AND t.OfficeCode = @StoreCode";
            parameters.Add("StoreCode", filter.StoreCode);
        }

        if (!string.IsNullOrEmpty(filter.ProductCode))
        {
            sql += " AND tl.ItemCode LIKE @ProductCode";
            parameters.Add("ProductCode", $"%{filter.ProductCode}%");
        }

        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        sql += @" ORDER BY t.TransactionDate DESC
                  OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
        parameters.Add("PageSize", filter.PageSize);

        var items = (await connection.QueryAsync<NebimSaleDto>(sql, parameters)).ToList();

        return PagedResult<NebimSaleDto>.Create(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<List<NebimSaleDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT TOP (@Limit)
                tl.ItemCode as ProductCode,
                i.ItemDescription as ProductName,
                SUM(tl.Qty) as Quantity,
                SUM(tl.NetAmount) as TotalAmount
            FROM trSalesTransaction t
            INNER JOIN trSalesTransactionLine tl ON t.TransactionID = tl.TransactionID
            INNER JOIN cdItem i ON tl.ItemCode = i.ItemCode
            WHERE t.TransactionDate >= @StartDate AND t.TransactionDate <= @EndDate
            GROUP BY tl.ItemCode, i.ItemDescription
            ORDER BY SUM(tl.NetAmount) DESC";

        var results = await connection.QueryAsync<NebimSaleDto>(sql, new { Limit = limit, StartDate = startDate, EndDate = endDate });
        return results.ToList();
    }

    public async Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT ISNULL(SUM(tl.NetAmount), 0)
            FROM trSalesTransaction t
            INNER JOIN trSalesTransactionLine tl ON t.TransactionID = tl.TransactionID
            WHERE t.TransactionDate >= @StartDate AND t.TransactionDate <= @EndDate";

        return await connection.ExecuteScalarAsync<decimal>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<Dictionary<DateTime, decimal>> GetDailySalesAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                CAST(t.TransactionDate AS DATE) as SaleDate,
                SUM(tl.NetAmount) as TotalAmount
            FROM trSalesTransaction t
            INNER JOIN trSalesTransactionLine tl ON t.TransactionID = tl.TransactionID
            WHERE t.TransactionDate >= @StartDate AND t.TransactionDate <= @EndDate
            GROUP BY CAST(t.TransactionDate AS DATE)
            ORDER BY SaleDate";

        var results = await connection.QueryAsync<(DateTime SaleDate, decimal TotalAmount)>(sql, new { StartDate = startDate, EndDate = endDate });
        return results.ToDictionary(r => r.SaleDate, r => r.TotalAmount);
    }

    #endregion

    #region Customers

    public async Task<PagedResult<NebimCustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT 
                c.CurrAccCode as CustomerCode,
                c.CurrAccDescription as CustomerName,
                c.Email,
                c.PhoneNumber as Phone,
                c.CityDescription as City,
                c.IsBlocked as IsActive
            FROM cdCurrAcc c
            WHERE c.CurrAccTypeCode = 1"; // 1 = Customer

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (c.CurrAccCode LIKE @Search OR c.CurrAccDescription LIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        sql += @" ORDER BY c.CurrAccCode
                  OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var items = (await connection.QueryAsync<NebimCustomerDto>(sql, parameters)).ToList();

        return PagedResult<NebimCustomerDto>.Create(items, page, pageSize, totalCount);
    }

    public async Task<NebimCustomerDto?> GetCustomerByCodeAsync(string customerCode, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT TOP 1
                c.CurrAccCode as CustomerCode,
                c.CurrAccDescription as CustomerName,
                c.Email,
                c.PhoneNumber as Phone,
                c.CityDescription as City,
                c.IsBlocked as IsActive
            FROM cdCurrAcc c
            WHERE c.CurrAccCode = @CustomerCode";

        return await connection.QueryFirstOrDefaultAsync<NebimCustomerDto>(sql, new { CustomerCode = customerCode });
    }

    public async Task<List<NebimCustomerDto>> GetTopCustomersAsync(int limit = 10, CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);

        var sql = @"
            SELECT TOP (@Limit)
                c.CurrAccCode as CustomerCode,
                c.CurrAccDescription as CustomerName,
                c.Email,
                c.PhoneNumber as Phone,
                c.CityDescription as City,
                ISNULL(SUM(tl.NetAmount), 0) as TotalPurchases
            FROM cdCurrAcc c
            LEFT JOIN trSalesTransaction t ON c.CurrAccCode = t.CurrAccCode
            LEFT JOIN trSalesTransactionLine tl ON t.TransactionID = tl.TransactionID
            WHERE c.CurrAccTypeCode = 1
            GROUP BY c.CurrAccCode, c.CurrAccDescription, c.Email, c.PhoneNumber, c.CityDescription
            ORDER BY SUM(tl.NetAmount) DESC";

        var results = await connection.QueryAsync<NebimCustomerDto>(sql, new { Limit = limit });
        return results.ToList();
    }

    #endregion

    #region Dashboard

    public async Task<int> GetTotalProductCountAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM cdItem");
    }

    public async Task<int> GetTotalCustomerCountAsync(CancellationToken ct = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await GetConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM cdCurrAcc WHERE CurrAccTypeCode = 1");
    }

    #endregion
}
