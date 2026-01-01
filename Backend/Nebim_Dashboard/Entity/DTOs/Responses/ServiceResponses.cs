using Entity.Enums;

namespace Entity.DTOs.Responses;

/// <summary>
/// Kullanıcı response (servis çıktısı)
/// </summary>
public class UserResponse
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
/// Chat oturumu response
/// </summary>
public class ChatSessionResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChatMessageResponse> Messages { get; set; } = new();
}

/// <summary>
/// Chat oturum özeti (liste için)
/// </summary>
public class ChatSessionSummary
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}

/// <summary>
/// Chat mesajı response
/// </summary>
public class ChatMessageResponse
{
    public int Id { get; set; }
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Düşük stok uyarısı
/// </summary>
public class LowStockAlertDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentQuantity { get; set; }
    public string Severity { get; set; } = "Warning"; // Warning, Critical
}
