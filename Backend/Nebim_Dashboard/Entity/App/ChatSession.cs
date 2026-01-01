using Entity.Base;

namespace Entity.App;

/// <summary>
/// AI Chat oturumu
/// Her oturum birden fazla mesaj içerebilir
/// </summary>
public class ChatSession : BaseEntity
{
    /// <summary>
    /// Oturum sahibi kullanıcı ID
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Oturum başlığı (ilk mesajdan otomatik oluşturulabilir)
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Oturum aktif mi? (devam eden konuşma)
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Son mesaj tarihi
    /// </summary>
    public DateTime? LastMessageAt { get; set; }
    
    /// <summary>
    /// Toplam mesaj sayısı
    /// </summary>
    public int MessageCount { get; set; } = 0;
    
    // Navigation Properties
    
    /// <summary>
    /// Oturum sahibi kullanıcı
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Oturumdaki mesajlar
    /// </summary>
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
