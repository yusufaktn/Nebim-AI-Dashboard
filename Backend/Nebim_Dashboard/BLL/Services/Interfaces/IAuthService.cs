using Entity.DTOs.Requests;
using Entity.DTOs.Responses;

namespace BLL.Services.Interfaces;

/// <summary>
/// Kimlik doğrulama servisi
/// Login, token yönetimi
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Token yenileme
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    
    /// <summary>
    /// Çıkış (refresh token iptal)
    /// </summary>
    Task<bool> LogoutAsync(int userId, CancellationToken ct = default);
}
