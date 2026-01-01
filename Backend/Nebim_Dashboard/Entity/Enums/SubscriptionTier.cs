namespace Entity.Enums;

/// <summary>
/// Subscription plan türleri.
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Ücretsiz plan - Sınırlı özellikler, sadece simulation.
    /// </summary>
    Free = 0,

    /// <summary>
    /// Profesyonel plan - Tüm özellikler, gerçek Nebim bağlantısı.
    /// </summary>
    Professional = 1,

    /// <summary>
    /// Kurumsal plan - Sınırsız kullanım, özel özellikler.
    /// </summary>
    Enterprise = 2
}
