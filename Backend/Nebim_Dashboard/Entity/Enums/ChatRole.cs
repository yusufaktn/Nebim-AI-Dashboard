namespace Entity.Enums;

/// <summary>
/// Chat mesajlarındaki rol
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// Kullanıcı mesajı
    /// </summary>
    User = 0,
    
    /// <summary>
    /// AI asistan mesajı
    /// </summary>
    Assistant = 1,
    
    /// <summary>
    /// Sistem mesajı (prompt)
    /// </summary>
    System = 2
}
