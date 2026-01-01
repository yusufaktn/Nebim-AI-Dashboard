namespace Entity.DTOs.Requests;

/// <summary>
/// AI Chat mesaj gönderme request
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// Kullanıcı mesajı
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Oturum ID (varsa mevcut oturuma devam eder)
    /// </summary>
    public int? SessionId { get; set; }
}

/// <summary>
/// Yeni chat oturumu oluşturma request
/// </summary>
public class CreateChatSessionRequest
{
    /// <summary>
    /// Oturum başlığı (opsiyonel)
    /// </summary>
    public string? Title { get; set; }
}
