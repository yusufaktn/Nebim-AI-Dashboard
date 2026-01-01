using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Entity.DTOs.Requests;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// Satış verilerini getiren capability.
/// </summary>
public class GetSalesCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetSalesCapability> _logger;

    public override string Name => "GetSales";
    public override string Version => "v1";
    public override string Description => "Belirli bir tarih aralığındaki satış verilerini getirir. Mağaza ve ürün bazlı filtreleme destekler.";
    public override string Category => "Sales";
    public override string RequiredTier => "Free";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "startDate", Type = "date", Description = "Başlangıç tarihi", IsRequired = false, DefaultValue = "30 gün önce" },
        new() { Name = "endDate", Type = "date", Description = "Bitiş tarihi", IsRequired = false, DefaultValue = "Bugün" },
        new() { Name = "storeCode", Type = "string", Description = "Mağaza kodu (opsiyonel)", IsRequired = false },
        new() { Name = "productCode", Type = "string", Description = "Ürün kodu filtresi (opsiyonel)", IsRequired = false },
        new() { Name = "page", Type = "int", Description = "Sayfa numarası", IsRequired = false, DefaultValue = "1" },
        new() { Name = "pageSize", Type = "int", Description = "Sayfa başına kayıt", IsRequired = false, DefaultValue = "20" }
    };

    public override List<string> ExampleQueries => new()
    {
        "Bu ayki satışları göster",
        "Son 7 günün satışları",
        "Mağaza-1'deki satışlar",
        "Ocak ayı satış raporu",
        "Bugünkü satışlar nelerdir?"
    };

    public GetSalesCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<GetSalesCapability> logger)
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
            var storeCode = GetParameter<string?>(parameters, "storeCode");
            var productCode = GetParameter<string?>(parameters, "productCode");
            var page = GetParameter<int?>(parameters, "page") ?? 1;
            var pageSize = GetParameter<int?>(parameters, "pageSize") ?? 20;

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            var filter = new SalesFilterRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                StoreCode = storeCode,
                ProductCode = productCode,
                Page = page,
                PageSize = pageSize
            };

            var result = await repository.GetSalesAsync(filter, ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "GetSales executed for tenant {TenantId}. Date range: {Start} - {End}. Results: {Count}",
                tenantId, startDate, endDate, result.TotalCount);

            return SuccessResult(new
            {
                sales = result.Items,
                pagination = new
                {
                    currentPage = result.CurrentPage,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    totalPages = result.TotalPages
                },
                dateRange = new { startDate, endDate },
                filters = new { storeCode, productCode }
            }, stopwatch.ElapsedMilliseconds, repository.DataSource, result.Items.Count);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSales failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "SALES_FETCH_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }

    public override ValidationResult ValidateParameters(JsonElement? parameters)
    {
        var (startDate, endDate) = GetDateRange(parameters);
        
        if (startDate > endDate)
        {
            return ValidationResult.Failure("Başlangıç tarihi bitiş tarihinden sonra olamaz");
        }

        if ((endDate - startDate).TotalDays > 365)
        {
            return ValidationResult.WithWarnings("Bir yıldan uzun tarih aralığı performans sorunlarına yol açabilir");
        }

        return ValidationResult.Success();
    }
}
