namespace Entity.DTOs.Requests;

/// <summary>
/// Kullanıcı giriş request
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Şifre
    /// </summary>
    public required string Password { get; set; }
}

/// <summary>
/// Kullanıcı kayıt request
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Tam ad
    /// </summary>
    public required string FullName { get; set; }
    
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Şifre
    /// </summary>
    public required string Password { get; set; }
    
    /// <summary>
    /// Şifre tekrar
    /// </summary>
    public required string ConfirmPassword { get; set; }
}

/// <summary>
/// Token yenileme request
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token
    /// </summary>
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Şifre değiştirme request
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Mevcut şifre
    /// </summary>
    public required string CurrentPassword { get; set; }
    
    /// <summary>
    /// Yeni şifre
    /// </summary>
    public required string NewPassword { get; set; }
    
    /// <summary>
    /// Yeni şifre tekrar
    /// </summary>
    public required string ConfirmNewPassword { get; set; }
}
