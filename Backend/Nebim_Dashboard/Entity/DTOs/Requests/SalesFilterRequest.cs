using Entity.DTOs.Common;

namespace Entity.DTOs.Requests;

/// <summary>
/// Satış listesi filtreleme request
/// </summary>
public class SalesFilterRequest : PagedRequest
{
    /// <summary>
    /// Başlangıç tarihi
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Bitiş tarihi
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Mağaza kodu filtresi
    /// </summary>
    public string? StoreCode { get; set; }
    
    /// <summary>
    /// Ürün kodu filtresi
    /// </summary>
    public string? ProductCode { get; set; }
    
    /// <summary>
    /// Müşteri kodu filtresi
    /// </summary>
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// Minimum tutar
    /// </summary>
    public decimal? MinAmount { get; set; }
    
    /// <summary>
    /// Maksimum tutar
    /// </summary>
    public decimal? MaxAmount { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi filtresi
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Satış türü (Retail/Wholesale)
    /// </summary>
    public string? SaleType { get; set; }
}
