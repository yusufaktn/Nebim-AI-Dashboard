using System.Security.Claims;
using Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Tüm controller'ların temel sınıfı
/// 
/// 🎓 AÇIKLAMA:
/// - Ortak özellikler burada tanımlanır
/// - CurrentUserId: JWT token'dan kullanıcı ID'si alır
/// - Tüm controller'lar bu sınıftan türer
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// JWT token'dan kullanıcı ID'sini al
    /// 
    /// 🎓 Claims:
    /// - Token içinde taşınan bilgiler
    /// - ClaimTypes.NameIdentifier = Kullanıcı ID
    /// - ClaimTypes.Role = Kullanıcı rolü
    /// </summary>
    protected int CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("uid")?.Value;
            
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    /// <summary>
    /// JWT token'dan tenant ID'sini al
    /// </summary>
    protected int? CurrentTenantId
    {
        get
        {
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    /// <summary>
    /// Kullanıcı email'i
    /// </summary>
    protected string? CurrentUserEmail 
        => User.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Kullanıcı rolü
    /// </summary>
    protected string? CurrentUserRole 
        => User.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>
    /// Kullanıcı admin mi?
    /// </summary>
    protected bool IsAdmin 
        => CurrentUserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Tenant admin mi?
    /// </summary>
    protected bool IsTenantAdmin
    {
        get
        {
            var claim = User.FindFirst("is_tenant_admin")?.Value;
            return bool.TryParse(claim, out var isAdmin) && isAdmin;
        }
    }

    /// <summary>
    /// Başarılı response döndür
    /// </summary>
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Success(data, message ?? "İşlem başarılı"));
    }

    /// <summary>
    /// Başarılı response döndür (veri yok)
    /// </summary>
    protected IActionResult Success(string? message = null)
    {
        return Ok(ApiResponse<object>.Success(null, message ?? "İşlem başarılı"));
    }

    /// <summary>
    /// Hata response döndür
    /// </summary>
    protected IActionResult Error(string message, int statusCode = 400)
    {
        var response = ApiResponse<object>.Fail(message);
        return StatusCode(statusCode, response);
    }
}
