using Entity.DTOs.AI;
using Entity.Enums;

namespace BLL.AI.Planner;

/// <summary>
/// AI Query Planner interface.
/// Doğal dil sorgusunu JSON Query Plan'a çevirir.
/// </summary>
public interface IQueryPlanner
{
    /// <summary>
    /// Kullanıcı sorgusunu analiz eder ve bir sorgu planı oluşturur.
    /// </summary>
    /// <param name="query">Kullanıcının doğal dil sorgusu</param>
    /// <param name="tenantId">Tenant ID (capability filtreleme için)</param>
    /// <param name="subscriptionTier">Subscription tier (capability erişimi için)</param>
    /// <param name="conversationHistory">Önceki konuşma geçmişi (context için, opsiyonel)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Query plan veya hata durumunda suggestions</returns>
    Task<QueryPlanResult> CreatePlanAsync(
        string query,
        int tenantId,
        SubscriptionTier subscriptionTier,
        List<ConversationMessage>? conversationHistory = null,
        CancellationToken ct = default);

    /// <summary>
    /// Mevcut capability listesini döndürür (prompt oluşturma için).
    /// </summary>
    string GetCapabilityDescriptions();
}

/// <summary>
/// Query plan sonucu.
/// </summary>
public class QueryPlanResult
{
    public bool Success { get; set; }
    public QueryPlanDto? Plan { get; set; }
    public string? Error { get; set; }
    public string? RawResponse { get; set; }
    public int TokensUsed { get; set; }
    public long LatencyMs { get; set; }
}

/// <summary>
/// Konuşma geçmişi mesajı.
/// </summary>
public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" veya "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
