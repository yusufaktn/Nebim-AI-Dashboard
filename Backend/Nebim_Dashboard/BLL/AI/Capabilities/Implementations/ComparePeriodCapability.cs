using System.Diagnostics;
using System.Text.Json;
using DAL.Providers;
using Entity.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities.Implementations;

/// <summary>
/// İki dönemi karşılaştıran capability.
/// </summary>
public class ComparePeriodCapability : BaseCapability
{
    private readonly INebimRepositoryFactory _repositoryFactory;
    private readonly ILogger<ComparePeriodCapability> _logger;

    public override string Name => "ComparePeriod";
    public override string Version => "v1";
    public override string Description => "İki farklı dönemi karşılaştırır. Satış, stok veya performans karşılaştırması için kullanılır.";
    public override string Category => "Analytics";
    public override string RequiredTier => "Professional";

    public override List<CapabilityParameterDto> Parameters => new()
    {
        new() { Name = "period1Start", Type = "date", Description = "Birinci dönem başlangıç", IsRequired = true },
        new() { Name = "period1End", Type = "date", Description = "Birinci dönem bitiş", IsRequired = true },
        new() { Name = "period2Start", Type = "date", Description = "İkinci dönem başlangıç", IsRequired = true },
        new() { Name = "period2End", Type = "date", Description = "İkinci dönem bitiş", IsRequired = true },
        new() { Name = "metric", Type = "string", Description = "Karşılaştırma metriği (sales, quantity)", IsRequired = false, DefaultValue = "sales", ExampleValues = new() { "sales", "quantity" } }
    };

    public override List<string> ExampleQueries => new()
    {
        "Bu ay ile geçen ayı karşılaştır",
        "Yılın ilk çeyreği ile ikinci çeyreği karşılaştır",
        "Geçen hafta bu haftadan farklı mı?",
        "Satış karşılaştırması yap",
        "Dönemsel performans karşılaştırması"
    };

    public ComparePeriodCapability(
        INebimRepositoryFactory repositoryFactory,
        ILogger<ComparePeriodCapability> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public override async Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Varsayılan: Bu ay vs geçen ay
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var period1Start = GetParameter<DateTime?>(parameters, "period1Start") ?? lastMonthStart;
            var period1End = GetParameter<DateTime?>(parameters, "period1End") ?? thisMonthStart.AddDays(-1);
            var period2Start = GetParameter<DateTime?>(parameters, "period2Start") ?? thisMonthStart;
            var period2End = GetParameter<DateTime?>(parameters, "period2End") ?? now;
            var metric = GetParameter<string?>(parameters, "metric") ?? "sales";

            var repository = await _repositoryFactory.CreateAsync(tenantId, ct);

            // Her iki dönem için satış verilerini al
            var period1Sales = await repository.GetTotalSalesAmountAsync(period1Start, period1End, ct);
            var period2Sales = await repository.GetTotalSalesAmountAsync(period2Start, period2End, ct);

            // Günlük satışları al
            var period1Daily = await repository.GetDailySalesAsync(period1Start, period1End, ct);
            var period2Daily = await repository.GetDailySalesAsync(period2Start, period2End, ct);

            // Değişim hesapla
            var change = period1Sales != 0 
                ? ((period2Sales - period1Sales) / period1Sales) * 100 
                : (period2Sales > 0 ? 100 : 0);

            stopwatch.Stop();

            _logger.LogInformation(
                "ComparePeriod executed for tenant {TenantId}. Period1: {P1S} - {P1E}, Period2: {P2S} - {P2E}",
                tenantId, period1Start, period1End, period2Start, period2End);

            return SuccessResult(new
            {
                comparison = new
                {
                    period1 = new
                    {
                        startDate = period1Start,
                        endDate = period1End,
                        totalSales = period1Sales,
                        dayCount = period1Daily.Count,
                        averageDaily = period1Daily.Count > 0 ? period1Sales / period1Daily.Count : 0
                    },
                    period2 = new
                    {
                        startDate = period2Start,
                        endDate = period2End,
                        totalSales = period2Sales,
                        dayCount = period2Daily.Count,
                        averageDaily = period2Daily.Count > 0 ? period2Sales / period2Daily.Count : 0
                    },
                    change = new
                    {
                        absolute = period2Sales - period1Sales,
                        percentage = Math.Round(change, 2),
                        trend = change > 0 ? "up" : (change < 0 ? "down" : "stable")
                    }
                },
                metric
            }, stopwatch.ElapsedMilliseconds, repository.DataSource);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "ComparePeriod failed for tenant {TenantId}", tenantId);
            return ErrorResult(ex.Message, "COMPARE_PERIOD_ERROR", stopwatch.ElapsedMilliseconds);
        }
    }

    public override ValidationResult ValidateParameters(JsonElement? parameters)
    {
        var period1Start = GetParameter<DateTime?>(parameters, "period1Start");
        var period1End = GetParameter<DateTime?>(parameters, "period1End");
        var period2Start = GetParameter<DateTime?>(parameters, "period2Start");
        var period2End = GetParameter<DateTime?>(parameters, "period2End");

        // Eğer belirtildiyse validate et
        if (period1Start.HasValue && period1End.HasValue && period1Start > period1End)
        {
            return ValidationResult.Failure("Birinci dönem başlangıcı bitişinden sonra olamaz");
        }

        if (period2Start.HasValue && period2End.HasValue && period2Start > period2End)
        {
            return ValidationResult.Failure("İkinci dönem başlangıcı bitişinden sonra olamaz");
        }

        return ValidationResult.Success();
    }
}
