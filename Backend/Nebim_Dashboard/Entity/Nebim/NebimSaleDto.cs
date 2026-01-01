namespace Entity.Nebim;

/// <summary>
/// Nebim V3'ten gelen satış bilgisi
/// Read-only DTO - Dapper ile sorgulanır
/// </summary>
public class NebimSaleDto
{
    /// <summary>
    /// Satış fişi numarası
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Satış tarihi
    /// </summary>
    public DateTime SaleDate { get; set; }
    
    /// <summary>
    /// Mağaza/Şube kodu
    /// </summary>
    public string StoreCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Mağaza/Şube adı
    /// </summary>
    public string? StoreName { get; set; }
    
    /// <summary>
    /// Ürün kodu
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Renk
    /// </summary>
    public string? ColorName { get; set; }
    
    /// <summary>
    /// Beden
    /// </summary>
    public string? SizeName { get; set; }
    
    /// <summary>
    /// Satış miktarı
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// İndirim tutarı
    /// </summary>
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Toplam tutar (KDV dahil)
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// KDV tutarı
    /// </summary>
    public decimal VatAmount { get; set; }
    
    /// <summary>
    /// Para birimi
    /// </summary>
    public string CurrencyCode { get; set; } = "TRY";
    
    /// <summary>
    /// Müşteri kodu (varsa)
    /// </summary>
    public string? CustomerCode { get; set; }
    
    /// <summary>
    /// Satış türü (Perakende/Toptan)
    /// </summary>
    public string SaleType { get; set; } = "Retail";
    
    /// <summary>
    /// Ödeme yöntemi
    /// </summary>
    public string? PaymentMethod { get; set; }
}
