namespace Entity.Enums;

/// <summary>
/// Kullanıcı rolleri
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standart kullanıcı - Dashboard görüntüleme
    /// </summary>
    User = 0,
    
    /// <summary>
    /// Yönetici - Tam yetki
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Sadece görüntüleme yetkisi
    /// </summary>
    Viewer = 2
}
