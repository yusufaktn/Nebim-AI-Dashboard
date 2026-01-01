namespace Entity.Enums;

/// <summary>
/// AI query planner tarafından belirlenen sorgu intent türleri.
/// </summary>
public enum QueryIntent
{
    /// <summary>
    /// Tanımlayıcı analiz - "Ne oldu?" sorusuna cevap verir.
    /// Örnek: "Bugünkü satışlar nelerdir?"
    /// </summary>
    Descriptive = 0,

    /// <summary>
    /// Teşhis analizi - "Neden oldu?" sorusuna cevap verir.
    /// Örnek: "Bu ürün neden az satıyor?"
    /// </summary>
    Diagnostic = 1,

    /// <summary>
    /// Karşılaştırmalı analiz - İki veya daha fazla veri setini karşılaştırır.
    /// Örnek: "Bu ay ile geçen ayı karşılaştır"
    /// </summary>
    Comparative = 2,

    /// <summary>
    /// Tahmine dayalı analiz - Trend bazlı gelecek tahmini.
    /// Örnek: "Bu ürün gelecek ay ne kadar satabilir?"
    /// Not: Kesinlik garantisi yoktur, trend bazlıdır.
    /// </summary>
    Predictive = 3,

    /// <summary>
    /// Kapsam dışı sorgu - Sistem bu soruyu yanıtlayamaz.
    /// Bu durumda SuggestedCapabilities ile en yakın öneriler sunulur.
    /// </summary>
    OutOfScope = 99
}
