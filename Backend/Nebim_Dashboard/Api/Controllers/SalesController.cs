using Api.Common;
using DAL.Repositories;
using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Satış raporu controller'ı
/// </summary>
[Authorize]
public class SalesController : BaseController
{
    private readonly INebimRepository _nebimRepository;

    public SalesController(INebimRepository nebimRepository)
    {
        _nebimRepository = nebimRepository;
    }

    /// <summary>
    /// Satış listesi (sayfalı)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<NebimSaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<NebimSaleDto>>> GetSales(
        [FromQuery] SalesFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _nebimRepository.GetSalesAsync(filter, ct);
        
        return Ok(new PagedApiResponse<NebimSaleDto>
        {
            Items = result.Items,
            Pagination = new PaginationMeta
            {
                Page = result.CurrentPage,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            }
        });
    }

    /// <summary>
    /// En çok satan ürünler
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(ApiResponse<List<NebimSaleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<NebimSaleDto>>>> GetTopSellingProducts(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        var products = await _nebimRepository.GetTopSellingProductsAsync(start, end, limit, ct);
        return Ok(ApiResponse<List<NebimSaleDto>>.Success(products));
    }

    /// <summary>
    /// Günlük satış özeti
    /// </summary>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<DateTime, decimal>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Dictionary<DateTime, decimal>>>> GetDailySales(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        var dailySales = await _nebimRepository.GetDailySalesAsync(start, end, ct);
        return Ok(ApiResponse<Dictionary<DateTime, decimal>>.Success(dailySales));
    }

    /// <summary>
    /// Toplam satış tutarı
    /// </summary>
    [HttpGet("total")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalSalesAmount(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        
        var total = await _nebimRepository.GetTotalSalesAmountAsync(start, end, ct);
        return Ok(ApiResponse<decimal>.Success(total));
    }

    /// <summary>
    /// Mağaza listesi (filtre için)
    /// </summary>
    [HttpGet("stores")]
    [ProducesResponseType(typeof(ApiResponse<List<StoreInfo>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StoreInfo>>>> GetStores(CancellationToken ct)
    {
        // Mock için sabit mağaza listesi
        var stores = new List<StoreInfo>
        {
            new() { Code = "STR001", Name = "İstanbul Kadıköy Mağaza" },
            new() { Code = "STR002", Name = "Ankara Kızılay Mağaza" },
            new() { Code = "STR003", Name = "İzmir Alsancak Mağaza" },
            new() { Code = "STR004", Name = "Bursa Nilüfer Mağaza" },
            new() { Code = "STR005", Name = "Antalya Konyaaltı Mağaza" }
        };
        
        return Ok(ApiResponse<List<StoreInfo>>.Success(stores));
    }
}

/// <summary>
/// Mağaza bilgisi
/// </summary>
public class StoreInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
