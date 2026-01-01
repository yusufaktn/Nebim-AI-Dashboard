using Entity.DTOs.Responses;

namespace BLL.Services.Interfaces;

/// <summary>
/// AI yanıt üretme servisi
/// Semantic Kernel + Google Gemini kullanarak AI yanıtları üretir
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Kullanıcı mesajına AI yanıtı üret
    /// </summary>
    /// <param name="userMessage">Kullanıcının mesajı</param>
    /// <param name="chatHistory">Önceki mesaj geçmişi (context için)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>AI yanıtı</returns>
    Task<string> GenerateResponseAsync(
        string userMessage, 
        List<ChatMessageResponse>? chatHistory = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Stok verileri hakkında soru sor
    /// </summary>
    Task<string> AskAboutStockAsync(string question, CancellationToken ct = default);
    
    /// <summary>
    /// Satış verileri hakkında soru sor
    /// </summary>
    Task<string> AskAboutSalesAsync(string question, CancellationToken ct = default);
}
