using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// Ürün detaylarını getiren capability.
/// </summary>
public class GetProductDetailsCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetProductDetailsCapability> _logger;

    public override string Name => "GetProductDetails";
    public override string Version => "v1";
    public override string Description => "Belirli bir ürünün detay bilgilerini, stok durumunu ve son satışlarını getirir.";
    public override string Category => "Product";
    public override string RequiredTier => "Free";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "productCode", Type = "string", Description = "Ürün kodu", IsRequired = true },
        new() { Name = "includeStock", Type = "bool", Description = "Stok bilgisi dahil edilsin mi?", IsRequired = false, DefaultValue = "true" },
        new() { Name = "includeSales", Type = "bool", Description = "Satış bilgisi dahil edilsin mi?", IsRequired = false, DefaultValue = "true" }
    };

    public override List<string> ExampleQueries => new()
    {
        "PRD-001 ürününün detayları",
        "Bu ürün hakkında bilgi ver",
        "Ürün kodu ABC123 nedir?",
        "Ürün detayı göster"
    };

    public GetProductDetailsCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<GetProductDetailsCapability> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public override async Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var productCode = GetParameter<string?>(parameters, "productCode");
            var includeStock = GetParameter<bool?>(parameters, "includeStock") ?? true;
            var includeSales = GetParameter<bool?>(parameters, "includeSales") ?? true;

            if (string.IsNullOrEmpty(productCode))
            {
                return ErrorResult("Ürün kodu gerekli", "MISSING_PRODUCT_CODE");
            }

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            // Ürün bilgisi
            var product = await repository.GetProductByCodeAsync(productCode, ct);
            if (product == null)
            {
                return ErrorResult($"Ürün bulunamadı: {productCode}", "PRODUCT_NOT_FOUND");
            }

            // Opsiyonel: Stok bilgisi
            object? stockInfo = null;
            if (includeStock)
            {
                var stocks = await repository.GetStockByProductCodeAsync(productCode, ct);
                stockInfo = new
                {
                    warehouses = stocks.Select(s => new
                    {
                        warehouseCode = s.WarehouseCode,
                        warehouseName = s.WarehouseName,
                        quantity = s.Quantity,
                        available = s.AvailableQuantity
                    }),
                    totalQuantity = stocks.Sum(s => s.Quantity),
                    totalAvailable = stocks.Sum(s => s.AvailableQuantity)
                };
            }

            // Opsiyonel: Son satışlar (son 30 gün)
            object? salesInfo = null;
            if (includeSales)
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30);
                var salesFilter = new Entity.DTOs.Requests.SalesFilterRequest
                {
                    ProductCode = productCode,
                    StartDate = startDate,
                    EndDate = endDate,
                    Page = 1,
                    PageSize = 100
                };
                var sales = await repository.GetSalesAsync(salesFilter, ct);

                salesInfo = new
                {
                    last30Days = new
                    {
                        totalQuantity = sales.Items.Sum(s => s.Quantity),
                        totalAmount = sales.Items.Sum(s => s.TotalAmount),
                        transactionCount = sales.Items.Count
                    }
                };
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "GetProductDetails executed for tenant {TenantId}. Product: {ProductCode}",
                tenantId, productCode);

            return SuccessResult(new
            {
                product = new
                {
                    code = product.ProductCode,
                    name = product.ProductName,
                    category = product.CategoryName,
                    brand = product.BrandName,
                    color = product.ColorName,
                    price = product.UnitPrice,
                    barcode = product.Barcode,
                    isActive = product.IsActive
                },
                stock = stockInfo,
                sales = salesInfo
            }, stopwatch.ElapsedMilliseconds, repository.DataSource);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetProductDetails failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "PRODUCT_DETAILS_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }

    public override ValidationResult ValidateParameters(JsonElement? parameters)
    {
        var productCode = GetParameter<string?>(parameters, "productCode");
        
        if (string.IsNullOrEmpty(productCode))
        {
            return ValidationResult.Failure("Ürün kodu gerekli");
        }

        return ValidationResult.Success();
    }
}
