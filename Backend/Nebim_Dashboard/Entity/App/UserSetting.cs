using Entity.Base;

namespace Entity.App;

/// <summary>
/// Kullanıcı ayarları
/// Her kullanıcının bir ayar kaydı olabilir (1-1 ilişki)
/// </summary>
public class UserSetting : BaseEntity
{
    /// <summary>
    /// Kullanıcı ID (Foreign Key + Primary Key olarak da kullanılabilir)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Tema tercihi (light/dark/system)
    /// </summary>
    public string Theme { get; set; } = "system";
    
    /// <summary>
    /// Dil tercihi (tr/en)
    /// </summary>
    public string Language { get; set; } = "tr";
    
    /// <summary>
    /// Varsayılan sayfa boyutu (tablo/listeler için)
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;
    
    /// <summary>
    /// Dashboard'da gösterilecek widget'lar (JSON array)
    /// </summary>
    public string? DashboardWidgets { get; set; }
    
    /// <summary>
    /// E-posta bildirimleri aktif mi?
    /// </summary>
    public bool EmailNotifications { get; set; } = true;
    
    /// <summary>
    /// AI chat geçmişi saklanıyor mu?
    /// </summary>
    public bool SaveChatHistory { get; set; } = true;
    
    // Navigation Properties
    
    /// <summary>
    /// Ayar sahibi kullanıcı
    /// </summary>
    public virtual User User { get; set; } = null!;
}
