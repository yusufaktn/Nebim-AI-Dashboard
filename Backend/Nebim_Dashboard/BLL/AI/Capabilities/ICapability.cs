using System.Text.Json;
using Entity.DTOs.AI;

namespace BLL.AI.Capabilities;

/// <summary>
/// AI sistemi tarafından kullanılabilecek bir yetenek.
/// Her capability, belirli bir veri işlemi veya analizi gerçekleştirir.
/// </summary>
public interface ICapability
{
    /// <summary>
    /// Capability'nin unique adı.
    /// Query plan'da referans için kullanılır.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Capability versiyonu.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Kullanıcı dostu açıklama.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Capability kategorisi (sales, stock, product, etc.)
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Bu capability için gerekli minimum subscription tier.
    /// </summary>
    string RequiredTier { get; }

    /// <summary>
    /// Parametre tanımları.
    /// AI'nın doğru parametreleri üretmesi için kullanılır.
    /// </summary>
    List<CapabilityParameterDto> Parameters { get; }

    /// <summary>
    /// Örnek doğal dil sorguları.
    /// AI'nın bu capability'yi ne zaman kullanacağını öğrenmesi için.
    /// </summary>
    List<string> ExampleQueries { get; }

    /// <summary>
    /// Capability'yi çalıştırır.
    /// </summary>
    /// <param name="tenantId">İşlemin yapılacağı tenant</param>
    /// <param name="parameters">JSON formatında parametreler</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>İşlem sonucu</returns>
    Task<CapabilityResultDto> ExecuteAsync(int tenantId, JsonElement? parameters, CancellationToken ct = default);

    /// <summary>
    /// Parametreleri validate eder.
    /// </summary>
    /// <param name="parameters">JSON formatında parametreler</param>
    /// <returns>Validation sonucu</returns>
    ValidationResult ValidateParameters(JsonElement? parameters);

    /// <summary>
    /// Capability bilgisini DTO olarak döndürür.
    /// </summary>
    CapabilityInfoDto GetInfo();
}

/// <summary>
/// Parametre validation sonucu.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };

    public static ValidationResult WithWarnings(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = warnings.ToList()
    };
}
