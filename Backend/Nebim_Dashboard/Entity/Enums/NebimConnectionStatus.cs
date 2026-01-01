namespace Entity.Enums;

/// <summary>
/// Tenant'ın Nebim ERP bağlantı durumu.
/// </summary>
public enum NebimConnectionStatus
{
    /// <summary>
    /// Bağlantı henüz yapılandırılmadı.
    /// Tenant simulation modunda çalışır.
    /// </summary>
    NotConfigured = 0,

    /// <summary>
    /// Bağlantı yapılandırıldı, test bekleniyor.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Bağlantı test ediliyor.
    /// </summary>
    Testing = 2,

    /// <summary>
    /// Bağlantı başarılı, sistem aktif.
    /// </summary>
    Connected = 3,

    /// <summary>
    /// Bağlantı başarısız, hata var.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Bağlantı kesildi (runtime hatası).
    /// </summary>
    Disconnected = 5
}
