using Entity.DTOs.Requests;
using Entity.DTOs.Responses;

namespace BLL.Services.Interfaces;

/// <summary>
/// AI Chat servisi
/// Kullanıcı mesajlarını işler, AI yanıtı üretir
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Yeni sohbet oturumu başlat
    /// </summary>
    Task<ChatSessionResponse> StartSessionAsync(int userId, string? title = null, CancellationToken ct = default);
    
    /// <summary>
    /// Mesaj gönder ve AI yanıtı al
    /// </summary>
    Task<ChatMessageResponse> SendMessageAsync(int sessionId, ChatRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Oturum geçmişini getir
    /// </summary>
    Task<ChatSessionResponse?> GetSessionAsync(int sessionId, CancellationToken ct = default);
    
    /// <summary>
    /// Kullanıcının tüm oturumlarını getir
    /// </summary>
    Task<List<ChatSessionSummary>> GetUserSessionsAsync(int userId, CancellationToken ct = default);
    
    /// <summary>
    /// Oturumu sil
    /// </summary>
    Task<bool> DeleteSessionAsync(int sessionId, CancellationToken ct = default);
}
