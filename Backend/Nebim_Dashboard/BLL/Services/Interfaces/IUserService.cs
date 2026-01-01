using Entity.App;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;

namespace BLL.Services.Interfaces;

/// <summary>
/// Kullanıcı işlemleri servisi
/// </summary>
public interface IUserService
{
    /// <summary>
    /// ID ile kullanıcı getir
    /// </summary>
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    
    /// <summary>
    /// Email ile kullanıcı getir
    /// </summary>
    Task<UserResponse?> GetByEmailAsync(string email, CancellationToken ct = default);
    
    /// <summary>
    /// Yeni kullanıcı oluştur
    /// </summary>
    Task<UserResponse> CreateAsync(RegisterRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Kullanıcı güncelle
    /// </summary>
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Kullanıcıyı pasif yap (soft delete)
    /// </summary>
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
}
