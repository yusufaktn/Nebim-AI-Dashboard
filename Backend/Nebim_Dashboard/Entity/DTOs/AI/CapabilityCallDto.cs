using System.Text.Json;

namespace Entity.DTOs.AI;

/// <summary>
/// Query plan içindeki tek bir capability çağrısı.
/// </summary>
public class CapabilityCallDto
{
    /// <summary>
    /// Capability adı (örn: "GetSales", "GetTopProducts").
    /// CapabilityRegistry'de kayıtlı olmalı.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capability versiyonu (örn: "v1", "v2").
    /// Belirtilmezse latest version kullanılır.
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Capability parametreleri (JSON).
    /// Her capability kendi parametre şemasını tanımlar.
    /// </summary>
    public JsonElement? Parameters { get; set; }

    /// <summary>
    /// Execution sırası. Aynı sıradakiler paralel çalışabilir.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Bu capability'nin bağımlı olduğu diğer capability'ler.
    /// Dependency chain oluşturmak için kullanılır.
    /// </summary>
    public List<string>? DependsOn { get; set; }

    /// <summary>
    /// Capability çağrısı için unique identifier.
    /// Dependency referansları için kullanılır.
    /// </summary>
    public string? CallId { get; set; }
}
