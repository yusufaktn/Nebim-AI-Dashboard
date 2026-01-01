using BLL.Helpers;
using BLL.Mappings;
using BLL.Services.Interfaces;
using DAL.UnitOfWork;
using Entity.App;
using Entity.DTOs.Requests;
using Entity.DTOs.Responses;
using Entity.Exceptions;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Kullanıcı servisi implementasyonu
/// 
/// 🎓 AÇIKLAMA:
/// - CRUD işlemleri için UnitOfWork pattern kullanır
/// - Şifre hashleme iş mantığı burada
/// - Entity ↔ DTO dönüşümleri burada
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    /// <summary>
    /// ID ile kullanıcı getir
    /// </summary>
    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Kullanıcı aranıyor: {UserId}", id);
        
        // 🎓 Repository<T>() metodu GenericRepository döndürür
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, ct);
        
        // 🎓 ToResponse() = Extension metod (MappingExtensions.cs)
        return user?.ToResponse();
    }
    
    /// <summary>
    /// Email ile kullanıcı getir
    /// </summary>
    public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
        
        _logger.LogDebug("Kullanıcı email ile aranıyor: {Email}", email);
        
        // 🎓 FirstOrDefaultAsync = Tek kayıt getir veya null
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
        
        return user?.ToResponse();
    }
    
    /// <summary>
    /// Yeni kullanıcı oluştur
    /// </summary>
    public async Task<UserResponse> CreateAsync(RegisterRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Yeni kullanıcı oluşturuluyor: {Email}", request.Email);
        
        // 1. Email kontrolü
        var existingUser = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), ct);
        
        if (existingUser)
        {
            _logger.LogWarning("Email zaten kayıtlı: {Email}", request.Email);
            throw new ConflictException($"Bu email adresi zaten kayıtlı: {request.Email}");
        }
        
        // 2. Şifre kontrolü
        if (request.Password != request.ConfirmPassword)
        {
            throw new ValidationException("Şifreler eşleşmiyor");
        }
        
        // 3. Entity oluştur
        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = PasswordHelper.HashPassword(request.Password), // 🎓 Şifre hashleme
            IsActive = true
        };
        
        // 4. Kaydet
        await _unitOfWork.Repository<User>().AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Kullanıcı oluşturuldu: {UserId}", user.Id);
        
        return user.ToResponse();
    }
    
    /// <summary>
    /// Kullanıcı güncelle
    /// </summary>
    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Kullanıcı güncelleniyor: {UserId}", id);
        
        // 1. Kullanıcıyı bul
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, ct);
        
        if (user == null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {id}");
        }
        
        // 2. Email değişiyorsa kontrol et
        if (!string.IsNullOrWhiteSpace(request.Email) && 
            request.Email.ToLower() != user.Email.ToLower())
        {
            var emailExists = await _unitOfWork.Repository<User>()
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != id, ct);
            
            if (emailExists)
            {
                throw new ConflictException($"Bu email adresi zaten kullanımda: {request.Email}");
            }
            
            user.Email = request.Email.ToLower().Trim();
        }
        
        // 3. Diğer alanları güncelle
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }
        
        // 4. Kaydet
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Kullanıcı güncellendi: {UserId}", id);
        
        return user.ToResponse();
    }
    
    /// <summary>
    /// Kullanıcıyı pasif yap (soft delete)
    /// </summary>
    public async Task<bool> DeactivateAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Kullanıcı deaktif ediliyor: {UserId}", id);
        
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, ct);
        
        if (user == null)
        {
            throw new NotFoundException($"Kullanıcı bulunamadı: {id}");
        }
        
        // 🎓 Soft Delete: Kaydı silmek yerine IsActive = false yapıyoruz
        // Avantajları:
        // - Veri kaybı yok
        // - Geri alınabilir
        // - Audit trail korunur
        user.IsActive = false;
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Kullanıcı deaktif edildi: {UserId}", id);
        
        return true;
    }
}
