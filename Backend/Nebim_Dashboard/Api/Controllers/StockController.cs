using Api.Common;
using BLL.Services.Interfaces;
using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Stok yönetimi controller'ı
/// </summary>
[Authorize]
public class StockController : BaseController
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>
    /// Stok listesi (sayfalı)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<NebimStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<NebimStockDto>>> GetStocks(
        [FromQuery] StockFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _stockService.GetStocksAsync(filter, ct);
        
        return Ok(new PagedApiResponse<NebimStockDto>
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
    /// Ürün detayı
    /// </summary>
    [HttpGet("products/{productCode}")]
    [ProducesResponseType(typeof(ApiResponse<NebimProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NebimProductDto>>> GetProduct(
        string productCode,
        CancellationToken ct)
    {
        var product = await _stockService.GetProductAsync(productCode, ct);
        
        if (product == null)
            return NotFound(ApiErrorResponse.Create($"Ürün bulunamadı: {productCode}"));
        
        return Ok(ApiResponse<NebimProductDto>.Success(product));
    }

    /// <summary>
    /// Ürün arama
    /// </summary>
    [HttpGet("products/search")]
    [ProducesResponseType(typeof(ApiResponse<List<NebimProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<NebimProductDto>>>> SearchProducts(
        [FromQuery] string term,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var products = await _stockService.SearchProductsAsync(term, limit, ct);
        return Ok(ApiResponse<List<NebimProductDto>>.Success(products));
    }

    /// <summary>
    /// Kategori listesi
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCategories(CancellationToken ct)
    {
        var categories = await _stockService.GetCategoriesAsync(ct);
        return Ok(ApiResponse<List<string>>.Success(categories));
    }

    /// <summary>
    /// Marka listesi
    /// </summary>
    [HttpGet("brands")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetBrands(CancellationToken ct)
    {
        var brands = await _stockService.GetBrandsAsync(ct);
        return Ok(ApiResponse<List<string>>.Success(brands));
    }
}
