namespace Entity.Enums;

/// <summary>
/// Nebim veri kaynağı türü.
/// </summary>
public enum NebimServerType
{
    /// <summary>
    /// Simülasyon modu - Fake data kullanılır.
    /// Development ve demo için.
    /// </summary>
    Simulation = 0,

    /// <summary>
    /// Gerçek Nebim V3 bağlantısı.
    /// Production için.
    /// </summary>
    Real = 1
}
