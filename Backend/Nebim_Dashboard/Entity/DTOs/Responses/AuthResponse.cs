using Entity.Enums;

namespace Entity.DTOs.Responses;

/// <summary>
/// Kullanıcı bilgisi response
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Login başarılı response
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT Access Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh Token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Access Token bitiş tarihi
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Kullanıcı bilgileri
    /// </summary>
    public UserDto User { get; set; } = null!;
}
