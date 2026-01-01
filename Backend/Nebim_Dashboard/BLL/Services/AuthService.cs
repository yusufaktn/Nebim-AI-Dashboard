using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BLL.Helpers;
using BLL.Mappings;
using BLL.Services.Interfaces;
using DAL.UnitOfWork;
using Entity.App;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Entity.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

/// <summary>
/// Kimlik doğrulama servisi
/// 
/// 🎓 AÇIKLAMA:
/// - JWT (JSON Web Token) tabanlı authentication
/// - Access Token: Kısa ömürlü (15 dk), API erişimi için
/// - Refresh Token: Uzun ömürlü (7 gün), Access Token yenilemek için
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Login denemesi: {Email}", request.Email);
        
        // 1. Kullanıcıyı bul
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), ct);
        
        if (user == null)
        {
            _logger.LogWarning("Kullanıcı bulunamadı: {Email}", request.Email);
            throw new UnauthorizedException("Email veya şifre hatalı");
        }
        
        // 2. Hesap aktif mi?
        if (!user.IsActive)
        {
            _logger.LogWarning("Deaktif hesap girişi: {Email}", request.Email);
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış");
        }
        
        // 3. Şifre kontrolü
        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Hatalı şifre: {Email}", request.Email);
            throw new UnauthorizedException("Email veya şifre hatalı");
        }
        
        // 4. Token oluştur
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        
        // 5. Refresh token'ı kaydet
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Login başarılı: {UserId}", user.Id);
        
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            User = user.ToDto()
        };
    }
    
    /// <summary>
    /// Token yenileme
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        _logger.LogInformation("Token yenileme isteği");
        
        // 1. Refresh token ile kullanıcıyı bul
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
        
        if (user == null)
        {
            _logger.LogWarning("Geçersiz refresh token");
            throw new UnauthorizedException("Geçersiz refresh token");
        }
        
        // 2. Token süresi dolmuş mu?
        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token süresi dolmuş: {UserId}", user.Id);
            throw new UnauthorizedException("Refresh token süresi dolmuş, lütfen tekrar giriş yapın");
        }
        
        // 3. Hesap aktif mi?
        if (!user.IsActive)
        {
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış");
        }
        
        // 4. Yeni token'lar oluştur
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();
        
        // 5. Yeni refresh token'ı kaydet
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Token yenilendi: {UserId}", user.Id);
        
        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            User = user.ToDto()
        };
    }
    
    /// <summary>
    /// Çıkış (refresh token iptal)
    /// </summary>
    public async Task<bool> LogoutAsync(int userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Logout: {UserId}", userId);
        
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, ct);
        
        if (user == null)
            return false;
        
        // Refresh token'ı temizle
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return true;
    }
    
    #region Private Methods
    
    /// <summary>
    /// JWT Access Token oluştur
    /// 
    /// 🎓 JWT Yapısı:
    /// - Header: Algoritma bilgisi (HS256)
    /// - Payload: Claims (kullanıcı bilgileri)
    /// - Signature: Header + Payload + Secret Key ile imza
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret bulunamadı");
        var issuer = jwtSettings["Issuer"] ?? "NebimDashboard";
        var audience = jwtSettings["Audience"] ?? "NebimDashboard";
        
        // 🎓 Claims: Token içinde taşınan bilgiler
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("uid", user.Id.ToString()) // Kısa erişim için
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    /// <summary>
    /// Rastgele Refresh Token oluştur
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    
    /// <summary>
    /// Token süresini config'den al
    /// </summary>
    private int GetTokenExpirationMinutes()
    {
        var minutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");
        return minutes > 0 ? minutes : 15; // Varsayılan 15 dakika
    }
    
    #endregion
}
