using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// Düşük stok uyarıları getiren capability.
/// </summary>
public class GetLowStockAlertsCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetLowStockAlertsCapability> _logger;

    public override string Name => "GetLowStockAlerts";
    public override string Version => "v1";
    public override string Description => "Stok miktarı belirlenen eşiğin altında olan ürünleri listeler. Kritik stok uyarıları için kullanılır.";
    public override string Category => "Stock";
    public override string RequiredTier => "Free";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "threshold", Type = "int", Description = "Stok uyarı eşiği", IsRequired = false, DefaultValue = "10" }
    };

    public override List<string> ExampleQueries => new()
    {
        "Düşük stoklu ürünler",
        "Stok uyarıları",
        "Kritik stoklar",
        "Bitmek üzere olan ürünler",
        "Stok seviyesi düşük olanlar"
    };

    public GetLowStockAlertsCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<GetLowStockAlertsCapability> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public override async Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var threshold = GetParameter<int?>(parameters, "threshold") ?? 10;

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            var lowStockItems = await repository.GetLowStockItemsAsync(threshold, ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "GetLowStockAlerts executed for tenant {TenantId}. Threshold: {Threshold}. Alerts: {Count}",
                tenantId, threshold, lowStockItems.Count);

            var alerts = lowStockItems.Select(item => new
            {
                productCode = item.ProductCode,
                productName = item.ProductName,
                warehouseCode = item.WarehouseCode,
                warehouseName = item.WarehouseName,
                currentQuantity = item.Quantity,
                availableQuantity = item.AvailableQuantity,
                threshold,
                severity = item.Quantity <= threshold / 2 ? "critical" : "warning"
            }).ToList();

            return SuccessResult(new
            {
                alerts,
                summary = new
                {
                    totalAlerts = alerts.Count,
                    criticalCount = alerts.Count(a => a.severity == "critical"),
                    warningCount = alerts.Count(a => a.severity == "warning"),
                    threshold
                }
            }, stopwatch.ElapsedMilliseconds, repository.DataSource, alerts.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetLowStockAlerts failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "LOW_STOCK_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }

    public override ValidationResult ValidateParameters(JsonElement? parameters)
    {
        var threshold = GetParameter<int?>(parameters, "threshold") ?? 10;
        
        if (threshold < 1)
        {
            return ValidationResult.Failure("Threshold en az 1 olmalı");
        }

        return ValidationResult.Success();
    }
}
