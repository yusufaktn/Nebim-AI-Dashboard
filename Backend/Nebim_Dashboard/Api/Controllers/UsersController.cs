using Api.Common;
using BLL.Services.Interfaces;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Entity.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Kullanıcı yönetimi controller'ı
/// 
/// 🎓 Role-Based Authorization:
/// - [Authorize(Roles = "Admin")] = Sadece Admin rolündekiler erişebilir
/// - Diğer kullanıcılar 403 Forbidden alır
/// </summary>
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Kullanıcı detayı
    /// 
    /// 🎓 Yetki kontrolü:
    /// - Admin tüm kullanıcıları görebilir
    /// - Normal kullanıcı sadece kendini görebilir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(
        int id,
        CancellationToken ct)
    {
        // Yetki kontrolü: Admin değilse sadece kendini görebilir
        if (!IsAdmin && CurrentUserId != id)
        {
            throw new ForbiddenException("Bu kullanıcıyı görüntüleme yetkiniz yok");
        }
        
        var user = await _userService.GetByIdAsync(id, ct);
        
        if (user == null)
            return NotFound(ApiErrorResponse.Create($"Kullanıcı bulunamadı: {id}"));
        
        return Ok(ApiResponse<UserResponse>.Success(user));
    }

    /// <summary>
    /// Kullanıcı güncelle
    /// 
    /// Admin veya kullanıcının kendisi güncelleyebilir
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Update(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        // Yetki kontrolü
        if (!IsAdmin && CurrentUserId != id)
        {
            throw new ForbiddenException("Bu kullanıcıyı güncelleme yetkiniz yok");
        }
        
        var user = await _userService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<UserResponse>.Success(user, "Kullanıcı güncellendi"));
    }

    /// <summary>
    /// Kullanıcıyı deaktif et (Soft Delete)
    /// 
    /// 🎓 [Authorize(Roles = "Admin")]: Sadece Admin erişebilir
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _userService.DeactivateAsync(id, ct);
        return NoContent();
    }
}
