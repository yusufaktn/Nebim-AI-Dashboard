namespace Entity.Nebim;

/// <summary>
/// Nebim V3'ten gelen müşteri bilgisi
/// Read-only DTO - Dapper ile sorgulanır
/// </summary>
public class NebimCustomerDto
{
    /// <summary>
    /// Müşteri kodu
    /// </summary>
    public string CustomerCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Müşteri adı
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Müşteri türü (Bireysel/Kurumsal)
    /// </summary>
    public string CustomerType { get; set; } = "Individual";
    
    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Şehir
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// İlçe
    /// </summary>
    public string? District { get; set; }
    
    /// <summary>
    /// Adres
    /// </summary>
    public string? Address { get; set; }
    
    /// <summary>
    /// Vergi numarası (Kurumsal için)
    /// </summary>
    public string? TaxNumber { get; set; }
    
    /// <summary>
    /// Toplam satış tutarı
    /// </summary>
    public decimal TotalSalesAmount { get; set; }
    
    /// <summary>
    /// Toplam satış adedi
    /// </summary>
    public int TotalSalesCount { get; set; }
    
    /// <summary>
    /// Son satış tarihi
    /// </summary>
    public DateTime? LastSaleDate { get; set; }
    
    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
