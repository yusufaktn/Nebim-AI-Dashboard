using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// En çok satan ürünleri getiren capability.
/// </summary>
public class GetTopProductsCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetTopProductsCapability> _logger;

    public override string Name => "GetTopProducts";
    public override string Version => "v1";
    public override string Description => "Belirli bir dönemde en çok satan ürünleri listeler. Satış tutarına göre sıralar.";
    public override string Category => "Sales";
    public override string RequiredTier => "Free";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "startDate", Type = "date", Description = "Başlangıç tarihi", IsRequired = false, DefaultValue = "30 gün önce" },
        new() { Name = "endDate", Type = "date", Description = "Bitiş tarihi", IsRequired = false, DefaultValue = "Bugün" },
        new() { Name = "limit", Type = "int", Description = "Kaç ürün listeleneceği", IsRequired = false, DefaultValue = "10" }
    };

    public override List<string> ExampleQueries => new()
    {
        "En çok satan 5 ürün",
        "Bu ay en çok satanlar",
        "Top 10 ürün",
        "En popüler ürünler hangileri?",
        "Çok satanlar listesi"
    };

    public GetTopProductsCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<GetTopProductsCapability> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public override async Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var (startDate, endDate) = GetDateRange(parameters);
            var limit = GetParameter<int?>(parameters, "limit") ?? 10;

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            var topProducts = await repository.GetTopSellingProductsAsync(startDate, endDate, limit, ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "GetTopProducts executed for tenant {TenantId}. Date range: {Start} - {End}. Limit: {Limit}",
                tenantId, startDate, endDate, limit);

            // Rank ekle
            var rankedProducts = topProducts.Select((p, index) => new
            {
                rank = index + 1,
                productCode = p.ProductCode,
                productName = p.ProductName,
                totalQuantity = p.Quantity,
                totalAmount = p.TotalAmount
            }).ToList();

            return SuccessResult(new
            {
                topProducts = rankedProducts,
                dateRange = new { startDate, endDate },
                limit
            }, stopwatch.ElapsedMilliseconds, repository.DataSource, rankedProducts.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetTopProducts failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "TOP_PRODUCTS_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }

    public override ValidationResult ValidateParameters(JsonElement? parameters)
    {
        var limit = GetParameter<int?>(parameters, "limit") ?? 10;
        
        if (limit < 1 || limit > 100)
        {
            return ValidationResult.Failure("Limit 1 ile 100 arasında olmalı");
        }

        return base.ValidateParameters(parameters);
    }
}
