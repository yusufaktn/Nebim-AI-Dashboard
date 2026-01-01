using Entity.Base;
using Entity.Enums;

namespace Entity.App;

/// <summary>
/// Uygulama kullanıcısı
/// PostgreSQL AppDB'de saklanır
/// </summary>
public class User : AuditableEntity
{
    /// <summary>
    /// Kullanıcının bağlı olduğu Tenant ID.
    /// Null = sistem kullanıcısı (admin), henüz tenant'a atanmamış.
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// Kullanıcı Tenant admin mi?
    /// Tenant admin'ler Nebim bağlantısı ve kullanıcı yönetimi yapabilir.
    /// </summary>
    public bool IsTenantAdmin { get; set; } = false;

    /// <summary>
    /// Kullanıcı e-posta adresi (unique)
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Şifrelenmiş parola (hash)
    /// </summary>
    public required string PasswordHash { get; set; }
    
    /// <summary>
    /// Kullanıcı tam adı
    /// </summary>
    public required string FullName { get; set; }
    
    /// <summary>
    /// Kullanıcı rolü
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
    
    /// <summary>
    /// Hesap aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Son giriş tarihi
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Refresh token (JWT yenileme için)
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Refresh token bitiş tarihi
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }
    
    // Navigation Properties

    /// <summary>
    /// Kullanıcının bağlı olduğu Tenant.
    /// Null olabilir (sistem kullanıcısı için).
    /// </summary>
    public virtual Tenant? Tenant { get; set; }
    
    /// <summary>
    /// Kullanıcının chat oturumları
    /// </summary>
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    /// <summary>
    /// Kullanıcının query geçmişi.
    /// </summary>
    public virtual ICollection<QueryHistory> QueryHistories { get; set; } = new List<QueryHistory>();
    
    /// <summary>
    /// Kullanıcı ayarları
    /// </summary>
    public virtual UserSetting? Setting { get; set; }
}
