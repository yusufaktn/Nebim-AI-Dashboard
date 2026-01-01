using Entity.Enums;

namespace Entity.DTOs.Responses;

/// <summary>
/// Chat yanıt response
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Oturum ID
    /// </summary>
    public int SessionId { get; set; }
    
    /// <summary>
    /// Mesaj ID
    /// </summary>
    public int MessageId { get; set; }
    
    /// <summary>
    /// AI yanıt mesajı
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Mesaj tipi
    /// </summary>
    public MessageType Type { get; set; }
    
    /// <summary>
    /// Ek veri (tablo, grafik verisi vb.)
    /// </summary>
    public object? Data { get; set; }
    
    /// <summary>
    /// İşlem süresi (ms)
    /// </summary>
    public int ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Yanıt zamanı
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Chat oturum özeti response
/// </summary>
public class ChatSessionDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}

/// <summary>
/// Chat mesaj geçmişi için DTO
/// </summary>
public class ChatMessageDto
{
    public int Id { get; set; }
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public object? Data { get; set; }
    public DateTime CreatedAt { get; set; }
}
