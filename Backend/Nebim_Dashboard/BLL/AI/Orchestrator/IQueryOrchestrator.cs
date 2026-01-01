using Entity.DTOs.AI;

namespace BLL.AI.Orchestrator;

/// <summary>
/// Query planını çalıştıran orchestrator interface.
/// </summary>
public interface IQueryOrchestrator
{
    /// <summary>
    /// Doğrulanmış bir query planını çalıştırır.
    /// </summary>
    /// <param name="plan">Çalıştırılacak query plan</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tüm capability sonuçlarını içeren response</returns>
    Task<OrchestrationResult> ExecuteAsync(QueryPlanDto plan, int tenantId, CancellationToken ct = default);
}

/// <summary>
/// Orchestration sonucu.
/// </summary>
public class OrchestrationResult
{
    public bool Success { get; set; }
    public List<CapabilityResultDto> Results { get; set; } = new();
    public long TotalExecutionTimeMs { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Tüm capability'ler başarılı oldu mu?
    /// </summary>
    public bool AllSucceeded => Results.All(r => r.IsSuccess);

    /// <summary>
    /// Toplam kayıt sayısı.
    /// </summary>
    public int TotalRecords => Results.Sum(r => r.RecordCount ?? 0);

    public static OrchestrationResult Successful(List<CapabilityResultDto> results, long executionTimeMs)
    {
        return new OrchestrationResult
        {
            Success = true,
            Results = results,
            TotalExecutionTimeMs = executionTimeMs
        };
    }

    public static OrchestrationResult Failed(string error)
    {
        return new OrchestrationResult
        {
            Success = false,
            Error = error
        };
    }
}
