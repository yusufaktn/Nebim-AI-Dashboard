using System.Text.Json;

namespace Entity.DTOs.AI;

/// <summary>
/// Tek bir capability execution sonucu.
/// </summary>
public class CapabilityResultDto
{
    /// <summary>
    /// Capability adı.
    /// </summary>
    public string CapabilityName { get; set; } = string.Empty;

    /// <summary>
    /// Kullanılan capability versiyonu.
    /// </summary>
    public string CapabilityVersion { get; set; } = string.Empty;

    /// <summary>
    /// Capability çağrısının unique ID'si (QueryPlan'daki CallId).
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// Execution başarılı mı?
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Capability'nin döndürdüğü veri.
    /// Her capability farklı yapıda veri döndürebilir.
    /// </summary>
    public JsonElement? Data { get; set; }

    /// <summary>
    /// Execution süresi (milisaniye).
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Hata durumunda hata mesajı.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Hata kodu (varsa).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Sonuç kaynak türü.
    /// </summary>
    public string DataSource { get; set; } = "real"; // "real" veya "simulation"

    /// <summary>
    /// Sonuçtaki kayıt sayısı (liste döndürüldüyse).
    /// </summary>
    public int? RecordCount { get; set; }
}
