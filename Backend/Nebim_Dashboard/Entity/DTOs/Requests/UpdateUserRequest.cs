namespace Entity.DTOs.Requests;

/// <summary>
/// Kullanıcı güncelleme request
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Tam ad
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// E-posta (değiştirilebilir)
    /// </summary>
    public string? Email { get; set; }
}
