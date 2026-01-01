using Entity.App;
using Entity.DTOs.Responses;

namespace BLL.Mappings;

/// <summary>
/// Entity ↔ DTO dönüşüm extension metodları
/// 
/// 🎓 AÇIKLAMA:
/// - AutoMapper yerine manuel mapping kullanıyoruz
/// - Neden? Daha az karmaşık, daha hızlı, debugging kolay
/// - Extension metod = Mevcut sınıflara yeni metod ekleme
/// 
/// Kullanım:
///   var userResponse = user.ToResponse();
///   var sessionResponse = session.ToResponse(includeMessages: true);
/// </summary>
public static class MappingExtensions
{
    #region User Mappings
    
    /// <summary>
    /// User entity → UserResponse DTO
    /// </summary>
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
    
    /// <summary>
    /// User entity → UserDto (Auth response için)
    /// </summary>
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
    
    #endregion
    
    #region ChatSession Mappings
    
    /// <summary>
    /// ChatSession entity → ChatSessionResponse DTO
    /// </summary>
    public static ChatSessionResponse ToResponse(this ChatSession session, bool includeMessages = true)
    {
        return new ChatSessionResponse
        {
            Id = session.Id,
            UserId = session.UserId,
            Title = session.Title,
            IsActive = session.IsActive,
            CreatedAt = session.CreatedAt,
            Messages = includeMessages 
                ? session.Messages.Select(m => m.ToResponse()).ToList() 
                : new List<ChatMessageResponse>()
        };
    }
    
    /// <summary>
    /// ChatSession entity → ChatSessionSummary (liste için)
    /// </summary>
    public static ChatSessionSummary ToSummary(this ChatSession session)
    {
        return new ChatSessionSummary
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            LastMessageAt = session.Messages.Any() 
                ? session.Messages.Max(m => m.CreatedAt) 
                : null,
            MessageCount = session.Messages.Count
        };
    }
    
    #endregion
    
    #region ChatMessage Mappings
    
    /// <summary>
    /// ChatMessage entity → ChatMessageResponse DTO
    /// </summary>
    public static ChatMessageResponse ToResponse(this ChatMessage message)
    {
        return new ChatMessageResponse
        {
            Id = message.Id,
            Role = message.Role,
            Content = message.Content,
            Type = message.Type,
            CreatedAt = message.CreatedAt
        };
    }
    
    #endregion
}
