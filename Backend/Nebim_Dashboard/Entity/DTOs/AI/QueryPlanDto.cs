using System.Text.Json;
using Entity.Enums;

namespace Entity.DTOs.AI;

/// <summary>
/// AI Query Planner tarafından üretilen sorgu planı.
/// Doğal dil sorgusunun yapılandırılmış representation'ı.
/// </summary>
public class QueryPlanDto
{
    /// <summary>
    /// Orijinal doğal dil sorgusu.
    /// </summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// AI tarafından belirlenen sorgu intent'i.
    /// </summary>
    public QueryIntent Intent { get; set; }

    /// <summary>
    /// AI'nın plan doğruluğuna olan güveni (0.0 - 1.0).
    /// 0.7 altı düşük güven olarak değerlendirilir.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Çalıştırılacak capability çağrıları listesi.
    /// Sıralı veya paralel olarak execute edilir.
    /// </summary>
    public List<CapabilityCallDto> Capabilities { get; set; } = new();

    /// <summary>
    /// Kapsam dışı sorularda önerilen en yakın capability'ler.
    /// Intent = OutOfScope olduğunda doldurulur.
    /// </summary>
    public List<SuggestedCapabilityDto>? SuggestedCapabilities { get; set; }

    /// <summary>
    /// Tenant'ın mevcut planında erişemediği bir capability gerekiyorsa true.
    /// Upsell fırsatı için kullanılır.
    /// </summary>
    public bool RequiresUpgrade { get; set; }

    /// <summary>
    /// Sonuçların nasıl birleştirileceğine dair ipucu.
    /// Örnek: "summarize", "compare", "list", "aggregate"
    /// </summary>
    public string? AggregationHint { get; set; }

    /// <summary>
    /// AI tarafından eklenen ek context veya açıklama.
    /// </summary>
    public string? PlannerNotes { get; set; }
}
