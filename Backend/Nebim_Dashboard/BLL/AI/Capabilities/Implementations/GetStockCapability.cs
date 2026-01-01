using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Entity.DTOs.Requests;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// Stok verilerini getiren capability.
/// </summary>
public class GetStockCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetStockCapability> _logger;

    public override string Name => "GetStock";
    public override string Version => "v1";
    public override string Description => "Depo ve ürün bazlı stok durumunu getirir. Düşük stok ve stokta olmayan ürünleri filtreleyebilir.";
    public override string Category => "Stock";
    public override string RequiredTier => "Free";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "warehouseCode", Type = "string", Description = "Depo kodu (opsiyonel)", IsRequired = false },
        new() { Name = "productCode", Type = "string", Description = "Ürün kodu filtresi", IsRequired = false },
        new() { Name = "minQuantity", Type = "int", Description = "Minimum stok miktarı filtresi", IsRequired = false },
        new() { Name = "inStockOnly", Type = "bool", Description = "Sadece stokta olanlar", IsRequired = false, DefaultValue = "false" },
        new() { Name = "page", Type = "int", Description = "Sayfa numarası", IsRequired = false, DefaultValue = "1" },
        new() { Name = "pageSize", Type = "int", Description = "Sayfa başına kayıt", IsRequired = false, DefaultValue = "20" }
    };

    public override List<string> ExampleQueries => new()
    {
        "Stok durumu",
        "Depo-1'deki stoklar",
        "Hangi ürünler stokta?",
        "Stok miktarları",
        "Ürün stoğu kontrol et"
    };

    public GetStockCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<GetStockCapability> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public override async Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var warehouseCode = GetParameter<string?>(parameters, "warehouseCode");
            var productCode = GetParameter<string?>(parameters, "productCode");
            var minQuantity = GetParameter<int?>(parameters, "minQuantity");
            var inStockOnly = GetParameter<bool?>(parameters, "inStockOnly") ?? false;
            var page = GetParameter<int?>(parameters, "page") ?? 1;
            var pageSize = GetParameter<int?>(parameters, "pageSize") ?? 20;

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            var filter = new StockFilterRequest
            {
                WarehouseCode = warehouseCode,
                ProductCode = productCode,
                MinQuantity = minQuantity,
                InStockOnly = inStockOnly,
                Page = page,
                PageSize = pageSize
            };

            var result = await repository.GetStocksAsync(filter, ct);
            var totalValue = await repository.GetTotalStockValueAsync(ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "GetStock executed for tenant {TenantId}. Results: {Count}, Total Value: {Value}",
                tenantId, result.TotalCount, totalValue);

            return SuccessResult(new
            {
                stocks = result.Items,
                summary = new
                {
                    totalValue,
                    totalItems = result.TotalCount
                },
                pagination = new
                {
                    currentPage = result.CurrentPage,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    totalPages = result.TotalPages
                },
                filters = new { warehouseCode, productCode, minQuantity, inStockOnly }
            }, stopwatch.ElapsedMilliseconds, repository.DataSource, result.Items.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetStock failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "STOCK_FETCH_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }
}
