namespace Entity.DTOs.AI;

/// <summary>
/// Business Intelligence sorgu isteği.
/// </summary>
public class BusinessQueryRequest
{
    /// <summary>
    /// Doğal dil sorgusu.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Önceki sorguların context'ini dahil et.
    /// Conversation flow için kullanılır.
    /// </summary>
    public bool IncludeContext { get; set; } = false;

    /// <summary>
    /// Önceki sorguların session ID'si (context için).
    /// </summary>
    public int? ContextSessionId { get; set; }

    /// <summary>
    /// Tercih edilen response formatı.
    /// </summary>
    public ResponseFormat PreferredFormat { get; set; } = ResponseFormat.Auto;
}

/// <summary>
/// Response format tercihi.
/// </summary>
public enum ResponseFormat
{
    /// <summary>
    /// Sistem en uygun formatı seçer.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Tablo formatı.
    /// </summary>
    Table = 1,

    /// <summary>
    /// Özet metin.
    /// </summary>
    Summary = 2,

    /// <summary>
    /// Grafik için data.
    /// </summary>
    Chart = 3,

    /// <summary>
    /// Ham JSON data.
    /// </summary>
    Raw = 4
}
