using Api.Common;
using BLL.Services.Interfaces;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Kimlik doğrulama controller'ı
/// 
/// 🎓 [AllowAnonymous]: Token olmadan erişilebilir
/// Login ve Register işlemleri için token gerekmez
/// </summary>
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Success(result, "Giriş başarılı"));
    }

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var user = await _userService.CreateAsync(request, ct);
        return CreatedAtAction(
            nameof(UsersController.GetById), 
            "Users",
            new { id = user.Id },
            ApiResponse<UserResponse>.Success(user, "Kayıt başarılı"));
    }

    /// <summary>
    /// Token yenileme
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(ApiResponse<AuthResponse>.Success(result));
    }

    /// <summary>
    /// Çıkış
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(CurrentUserId, ct);
        return Ok(ApiResponse<string>.Success("Çıkış yapıldı"));
    }

    /// <summary>
    /// Mevcut kullanıcı bilgisi
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser(CancellationToken ct)
    {
        var user = await _userService.GetByIdAsync(CurrentUserId, ct);
        return Ok(ApiResponse<UserResponse>.Success(user!));
    }
}
