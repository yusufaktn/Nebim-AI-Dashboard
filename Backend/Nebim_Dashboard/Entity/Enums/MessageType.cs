namespace Entity.Enums;

/// <summary>
/// Mesaj içerik tipi
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Düz metin mesaj
    /// </summary>
    Text = 0,
    
    /// <summary>
    /// Tablo veya veri içeren mesaj
    /// </summary>
    Data = 1,
    
    /// <summary>
    /// Grafik/chart içeren mesaj
    /// </summary>
    Chart = 2,
    
    /// <summary>
    /// Hata mesajı
    /// </summary>
    Error = 3
}
