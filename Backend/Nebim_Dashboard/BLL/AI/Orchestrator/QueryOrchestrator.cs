using System.Diagnostics;
using BLL.AI.Capabilities;
using Entity.DTOs.AI;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Orchestrator;

/// <summary>
/// Query orchestrator implementasyonu.
/// Dependency graph'a göre capability'leri sırayla veya paralel çalıştırır.
/// </summary>
public class QueryOrchestrator : IQueryOrchestrator
{
    private readonly ICapabilityRegistry _capabilityRegistry;
    private readonly ILogger<QueryOrchestrator> _logger;

    public QueryOrchestrator(
        ICapabilityRegistry capabilityRegistry,
        ILogger<QueryOrchestrator> logger)
    {
        _capabilityRegistry = capabilityRegistry;
        _logger = logger;
    }

    public async Task<OrchestrationResult> ExecuteAsync(
        QueryPlanDto plan, 
        int tenantId, 
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<CapabilityResultDto>();
        var completedCapabilities = new HashSet<string>();

        try
        {
            // Capability'leri order'a göre sırala
            var sortedCapabilities = plan.Capabilities
                .OrderBy(c => c.Order)
                .ToList();

            // Execution grupları oluştur (dependency'si olmayanlar paralel çalışabilir)
            var executionGroups = BuildExecutionGroups(sortedCapabilities);

            foreach (var group in executionGroups)
            {
                // Her gruptaki capability'leri paralel çalıştır
                var tasks = group.Select(capCall => ExecuteCapabilityAsync(
                    capCall, 
                    tenantId, 
                    completedCapabilities, 
                    results,
                    ct));

                var groupResults = await Task.WhenAll(tasks);
                results.AddRange(groupResults);

                // Tamamlananları işaretle
                foreach (var capCall in group)
                {
                    completedCapabilities.Add(capCall.Name);
                }

                // Herhangi biri başarısız olduysa devam etme
                if (groupResults.Any(r => !r.IsSuccess))
                {
                    _logger.LogWarning(
                        "Orchestration stopped due to capability failure. Completed: {Completed}/{Total}",
                        completedCapabilities.Count, plan.Capabilities.Count);
                    break;
                }
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "Orchestration completed for tenant {TenantId}. Capabilities: {Count}, Success: {Success}, Time: {Time}ms",
                tenantId, results.Count, results.All(r => r.IsSuccess), stopwatch.ElapsedMilliseconds);

            return OrchestrationResult.Successful(results, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Orchestration cancelled for tenant {TenantId}", tenantId);
            return OrchestrationResult.Failed("İşlem iptal edildi");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Orchestration failed for tenant {TenantId}", tenantId);
            return OrchestrationResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Capability'leri dependency'lere göre gruplar.
    /// Aynı gruptakiler paralel çalışabilir.
    /// </summary>
    private List<List<CapabilityCallDto>> BuildExecutionGroups(List<CapabilityCallDto> capabilities)
    {
        var groups = new List<List<CapabilityCallDto>>();
        var remaining = new List<CapabilityCallDto>(capabilities);
        var completed = new HashSet<string>();

        while (remaining.Any())
        {
            // Bu turda çalışabilecekler (dependency'leri tamamlanmış olanlar)
            var ready = remaining
                .Where(c => c.DependsOn == null || c.DependsOn.All(d => completed.Contains(d)))
                .ToList();

            if (!ready.Any())
            {
                // Circular dependency veya missing dependency
                _logger.LogWarning(
                    "Circular or missing dependency detected. Remaining: {Remaining}",
                    string.Join(", ", remaining.Select(r => r.Name)));
                
                // Kalanları tek grup olarak ekle
                groups.Add(remaining);
                break;
            }

            groups.Add(ready);
            
            foreach (var cap in ready)
            {
                completed.Add(cap.Name);
                remaining.Remove(cap);
            }
        }

        return groups;
    }

    private async Task<CapabilityResultDto> ExecuteCapabilityAsync(
        CapabilityCallDto capCall,
        int tenantId,
        HashSet<string> completedCapabilities,
        List<CapabilityResultDto> previousResults,
        CancellationToken ct)
    {
        var capability = _capabilityRegistry.GetCapability(capCall.Name, capCall.Version);

        if (capability == null)
        {
            _logger.LogError("Capability not found: {Name}@{Version}", capCall.Name, capCall.Version);
            return new CapabilityResultDto
            {
                CapabilityName = capCall.Name,
                IsSuccess = false,
                ErrorMessage = $"Capability bulunamadı: {capCall.Name}@{capCall.Version}",
                ErrorCode = "CAPABILITY_NOT_FOUND"
            };
        }

        try
        {
            _logger.LogDebug(
                "Executing capability {Name}@{Version} for tenant {TenantId}",
                capCall.Name, capCall.Version, tenantId);

            var result = await capability.ExecuteAsync(tenantId, capCall.Parameters, ct);
            result.CapabilityName = capCall.Name;
            result.CapabilityVersion = capCall.Version;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Capability {Name} execution failed for tenant {TenantId}",
                capCall.Name, tenantId);

            return new CapabilityResultDto
            {
                CapabilityName = capCall.Name,
                CapabilityVersion = capCall.Version,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "CAPABILITY_EXECUTION_ERROR"
            };
        }
    }
}
