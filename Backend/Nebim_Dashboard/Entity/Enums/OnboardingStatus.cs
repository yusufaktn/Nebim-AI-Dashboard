namespace Entity.Enums;

/// <summary>
/// Tenant onboarding (ilk kurulum) durumu.
/// </summary>
public enum OnboardingStatus
{
    /// <summary>
    /// Onboarding henüz başlamadı.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Firma bilgileri girildi, Nebim bağlantısı bekleniyor.
    /// </summary>
    PendingNebimConnection = 1,

    /// <summary>
    /// Nebim bağlantısı test ediliyor.
    /// </summary>
    TestingConnection = 2,

    /// <summary>
    /// Bağlantı başarısız, düzeltme bekleniyor.
    /// </summary>
    ConnectionFailed = 3,

    /// <summary>
    /// Onboarding tamamlandı, sistem kullanıma hazır.
    /// </summary>
    Completed = 4
}
