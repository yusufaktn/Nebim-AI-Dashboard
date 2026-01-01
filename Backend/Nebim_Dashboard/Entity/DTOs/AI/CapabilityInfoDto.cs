namespace Entity.DTOs.AI;

/// <summary>
/// Capability bilgisi (frontend'e sunulacak).
/// </summary>
public class CapabilityInfoDto
{
    /// <summary>
    /// Capability unique adı.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mevcut versiyonlar listesi.
    /// </summary>
    public List<string> Versions { get; set; } = new() { "v1" };

    /// <summary>
    /// Aktif (latest) versiyon.
    /// </summary>
    public string ActiveVersion { get; set; } = "v1";

    /// <summary>
    /// Kullanıcı dostu açıklama.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Kategorisi (sales, stock, product, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Parametre tanımları.
    /// </summary>
    public List<CapabilityParameterDto> Parameters { get; set; } = new();

    /// <summary>
    /// Örnek sorgular.
    /// </summary>
    public List<string> ExampleQueries { get; set; } = new();

    /// <summary>
    /// Bu capability hangi subscription tier'dan itibaren erişilebilir?
    /// </summary>
    public string RequiredTier { get; set; } = "Free";

    /// <summary>
    /// Deprecated mi?
    /// </summary>
    public bool IsDeprecated { get; set; }

    /// <summary>
    /// Deprecation notu (varsa).
    /// </summary>
    public string? DeprecationNote { get; set; }
}

/// <summary>
/// Capability parametre tanımı.
/// </summary>
public class CapabilityParameterDto
{
    /// <summary>
    /// Parametre adı.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Veri tipi (string, int, date, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Açıklama.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Zorunlu mu?
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Varsayılan değer (varsa).
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Örnek değerler.
    /// </summary>
    public List<string>? ExampleValues { get; set; }
}
