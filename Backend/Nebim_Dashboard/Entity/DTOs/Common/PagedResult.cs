namespace Entity.DTOs.Common;

/// <summary>
/// Sayfalanmış sonuçlar için wrapper
/// </summary>
/// <typeparam name="T">Liste elemanı tipi</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Sayfa verileri
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// Mevcut sayfa numarası (1'den başlar)
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Sayfa başına kayıt sayısı
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Toplam sayfa sayısı
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Önceki sayfa var mı?
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    /// Sonraki sayfa var mı?
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
    
    /// <summary>
    /// Boş sayfalı sonuç
    /// </summary>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
    {
        return new PagedResult<T>
        {
            Items = new List<T>(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = 0
        };
    }
    
    /// <summary>
    /// Sayfalı sonuç oluştur
    /// </summary>
    public static PagedResult<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Sayfalama parametreleri için base request
/// </summary>
public abstract class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 20;
    
    /// <summary>
    /// Sayfa numarası (1'den başlar)
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }
    
    /// <summary>
    /// Sayfa başına kayıt sayısı (max 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : (value > 100 ? 100 : value);
    }
    
    /// <summary>
    /// Sıralama alanı
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Sıralama yönü (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Azalan sıralama mı?
    /// </summary>
    public bool IsDescending => SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
    
    /// <summary>
    /// Skip değeri (pagination için)
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}
