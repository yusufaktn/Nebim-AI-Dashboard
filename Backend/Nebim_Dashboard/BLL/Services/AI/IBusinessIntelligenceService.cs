using Entity.DTOs.AI;

namespace BLL.Services.AI;

/// <summary>
/// İş zekası servisi interface.
/// Ana giriş noktası - kullanıcı sorgusunu alır, planlar, doğrular ve çalıştırır.
/// </summary>
public interface IBusinessIntelligenceService
{
    /// <summary>
    /// Kullanıcı sorgusunu işler ve sonuç döndürür.
    /// </summary>
    /// <param name="request">Kullanıcı sorgusu</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>İş zekası yanıtı</returns>
    Task<BusinessQueryResponse> ProcessQueryAsync(
        BusinessQueryRequest request, 
        int tenantId, 
        int userId,
        CancellationToken ct = default);

    /// <summary>
    /// Mevcut capability'leri listeler.
    /// </summary>
    Task<List<CapabilityInfoDto>> GetAvailableCapabilitiesAsync(
        int tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Sorgu geçmişini döndürür.
    /// </summary>
    Task<List<QueryHistorySummaryDto>> GetQueryHistoryAsync(
        int tenantId,
        int? userId = null,
        int limit = 20,
        CancellationToken ct = default);
}

/// <summary>
/// Sorgu geçmişi özeti.
/// </summary>
public class QueryHistorySummaryDto
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int TokensUsed { get; set; }
}
