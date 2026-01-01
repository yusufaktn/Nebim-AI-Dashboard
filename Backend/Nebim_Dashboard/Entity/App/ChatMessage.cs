using Entity.Base;
using Entity.Enums;

namespace Entity.App;

/// <summary>
/// Chat mesajı
/// Kullanıcı ve AI arasındaki her bir mesaj
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// Ait olduğu oturum ID
    /// </summary>
    public int SessionId { get; set; }
    
    /// <summary>
    /// Mesaj rolü (User/Assistant/System)
    /// </summary>
    public ChatRole Role { get; set; }
    
    /// <summary>
    /// Mesaj içeriği
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// Mesaj tipi (Text/Data/Chart/Error)
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Text;
    
    /// <summary>
    /// Ek veri (JSON formatında - tablo, grafik verisi vb.)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// AI tarafından kullanılan token sayısı
    /// </summary>
    public int? TokenCount { get; set; }
    
    /// <summary>
    /// İşlem süresi (milisaniye)
    /// </summary>
    public int? ProcessingTimeMs { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Ait olduğu oturum
    /// </summary>
    public virtual ChatSession Session { get; set; } = null!;
}
