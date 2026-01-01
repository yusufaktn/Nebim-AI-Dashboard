namespace Entity.Nebim;

/// <summary>
/// Nebim V3'ten gelen ürün bilgisi
/// Read-only DTO - Dapper ile sorgulanır
/// </summary>
public class NebimProductDto
{
    /// <summary>
    /// Ürün kodu (ItemCode)
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ürün grubu/kategorisi
    /// </summary>
    public string? CategoryName { get; set; }
    
    /// <summary>
    /// Alt kategori
    /// </summary>
    public string? SubCategoryName { get; set; }
    
    /// <summary>
    /// Marka
    /// </summary>
    public string? BrandName { get; set; }
    
    /// <summary>
    /// Renk
    /// </summary>
    public string? ColorName { get; set; }
    
    /// <summary>
    /// Beden
    /// </summary>
    public string? SizeName { get; set; }
    
    /// <summary>
    /// Barkod
    /// </summary>
    public string? Barcode { get; set; }
    
    /// <summary>
    /// Satış fiyatı
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Birim fiyat (SalePrice alias)
    /// </summary>
    public decimal UnitPrice => SalePrice;
    
    /// <summary>
    /// Maliyet fiyatı
    /// </summary>
    public decimal? CostPrice { get; set; }
    
    /// <summary>
    /// Para birimi
    /// </summary>
    public string CurrencyCode { get; set; } = "TRY";
    
    /// <summary>
    /// KDV oranı
    /// </summary>
    public decimal VatRate { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
