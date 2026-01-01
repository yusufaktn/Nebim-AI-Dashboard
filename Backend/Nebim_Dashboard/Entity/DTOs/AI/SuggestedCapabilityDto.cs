namespace Entity.DTOs.AI;

/// <summary>
/// Kapsam dışı sorularda önerilen capability.
/// </summary>
public class SuggestedCapabilityDto
{
    /// <summary>
    /// Önerilen capability adı.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capability açıklaması.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Orijinal sorguya uygunluk skoru (0.0 - 1.0).
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Bu capability ile sorulabilecek örnek soru.
    /// </summary>
    public string ExampleQuery { get; set; } = string.Empty;
}
