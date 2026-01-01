namespace Entity.Nebim;

/// <summary>
/// Nebim V3'ten gelen stok bilgisi
/// Read-only DTO - Dapper ile sorgulanır
/// </summary>
public class NebimStockDto
{
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
    /// Depo kodu
    /// </summary>
    public string WarehouseCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Depo adı
    /// </summary>
    public string? WarehouseName { get; set; }
    
    /// <summary>
    /// Mevcut stok miktarı
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Rezerve miktar
    /// </summary>
    public decimal ReservedQuantity { get; set; }
    
    /// <summary>
    /// Kullanılabilir miktar (Quantity - ReservedQuantity)
    /// </summary>
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    
    /// <summary>
    /// Birim
    /// </summary>
    public string UnitCode { get; set; } = "ADET";
    
    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime? LastUpdatedAt { get; set; }
}
