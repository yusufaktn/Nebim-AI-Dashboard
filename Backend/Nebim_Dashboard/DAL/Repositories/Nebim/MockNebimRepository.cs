using Entity.DTOs.Common;
using Entity.DTOs.Requests;
using Entity.Nebim;

namespace DAL.Repositories.Nebim;

/// <summary>
/// Merkezi Mock Data Provider - Tüm sistemin kullandığı tek veri kaynağı
/// Static veriler sayesinde tüm repository'ler aynı verileri kullanır
/// </summary>
public static class MockDataProvider
{
    private static readonly Lazy<List<NebimProductDto>> _products = 
        new(() => GenerateMockProducts(), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<List<NebimStockDto>> _stocks = 
        new(() => GenerateMockStocks(), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<List<NebimSaleDto>> _sales = 
        new(() => GenerateMockSales(), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<List<NebimCustomerDto>> _customers = 
        new(() => GenerateMockCustomers(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static IReadOnlyList<NebimProductDto> Products => _products.Value;
    public static IReadOnlyList<NebimStockDto> Stocks => _stocks.Value;
    public static IReadOnlyList<NebimSaleDto> Sales => _sales.Value;
    public static IReadOnlyList<NebimCustomerDto> Customers => _customers.Value;

    private static List<NebimProductDto> GenerateMockProducts()
    {
        var brands = new[] { "Zara", "H&M", "Mango", "Bershka", "Pull&Bear", "Stradivarius", "Massimo Dutti", "Koton", "LC Waikiki", "Colin's" };
        var categories = new[] { "Gömlek", "Pantolon", "Ceket", "Mont", "Elbise", "Etek", "Kazak", "Tişört", "Ayakkabı", "Çanta" };
        var subCategories = new Dictionary<string, string[]> {
            { "Gömlek", new[] { "Klasik Gömlek", "Slim Fit Gömlek", "Oversize Gömlek" }},
            { "Pantolon", new[] { "Jean Pantolon", "Kumaş Pantolon", "Kargo Pantolon" }},
            { "Ceket", new[] { "Blazer", "Deri Ceket", "Kot Ceket" }},
            { "Mont", new[] { "Kaban", "Parka", "Şişme Mont", "Trençkot" }},
            { "Elbise", new[] { "Günlük Elbise", "Gece Elbisesi", "Ofis Elbisesi" }},
            { "Etek", new[] { "Mini Etek", "Midi Etek", "Maxi Etek" }},
            { "Kazak", new[] { "V Yaka Kazak", "Boğazlı Kazak", "Hırka" }},
            { "Tişört", new[] { "Basic Tişört", "Baskılı Tişört", "Polo Yaka" }},
            { "Ayakkabı", new[] { "Sneaker", "Bot", "Topuklu", "Loafer" }},
            { "Çanta", new[] { "El Çantası", "Sırt Çantası", "Omuz Çantası" }}
        };
        var colors = new[] { "Siyah", "Beyaz", "Lacivert", "Kahverengi", "Gri", "Bej", "Kırmızı", "Mavi", "Yeşil", "Bordo" };
        var sizes = new[] { "XS", "S", "M", "L", "XL", "XXL" };
        var products = new List<NebimProductDto>();
        var random = new Random(42);
        var productId = 1;
        
        foreach (var brand in brands)
        {
            foreach (var category in categories)
            {
                var subs = subCategories[category];
                var sub = subs[random.Next(subs.Length)];
                var color = colors[random.Next(colors.Length)];
                var size = sizes[random.Next(sizes.Length)];
                var basePrice = random.Next(100, 2500);
                
                products.Add(new NebimProductDto
                {
                    ProductCode = $"PRD{productId:D5}",
                    ProductName = $"{brand} {sub} - {color}",
                    CategoryName = category,
                    SubCategoryName = sub,
                    BrandName = brand,
                    ColorName = color,
                    SizeName = size,
                    SalePrice = basePrice,
                    CostPrice = basePrice * 0.5m,
                    Barcode = $"86900{productId:D8}",
                    IsActive = random.NextDouble() > 0.05
                });
                productId++;
            }
        }
        
        return products;
    }

    private static List<NebimStockDto> GenerateMockStocks()
    {
        var warehouses = new[] { 
            ("WH001", "Merkez Depo - İstanbul"), 
            ("WH002", "İstanbul Kadıköy Mağaza"), 
            ("WH003", "Ankara Kızılay Mağaza"),
            ("WH004", "İzmir Alsancak Mağaza"),
            ("WH005", "Bursa Nilüfer Mağaza"),
            ("WH006", "Antalya Konyaaltı Mağaza")
        };
        var stocks = new List<NebimStockDto>();
        var random = new Random(42);
        var products = Products;
        
        foreach (var product in products)
        {
            var warehouseCount = random.Next(2, 5);
            var selectedWarehouses = warehouses.OrderBy(_ => random.Next()).Take(warehouseCount);
            
            foreach (var (code, name) in selectedWarehouses)
            {
                var stockRoll = random.NextDouble();
                int qty;
                if (stockRoll < 0.10) qty = 0;
                else if (stockRoll < 0.30) qty = random.Next(1, 10);
                else qty = random.Next(10, 150);
                
                stocks.Add(new NebimStockDto
                {
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    ColorName = product.ColorName,
                    SizeName = product.SizeName,
                    WarehouseCode = code,
                    WarehouseName = name,
                    Quantity = qty,
                    ReservedQuantity = qty > 5 ? random.Next(0, Math.Min(qty / 4, 10)) : 0,
                    UnitCode = "ADET",
                    LastUpdatedAt = DateTime.UtcNow.AddHours(-random.Next(0, 168))
                });
            }
        }
        
        return stocks;
    }

    private static List<NebimSaleDto> GenerateMockSales()
    {
        var stores = new[] { 
            ("STR001", "İstanbul Kadıköy Mağaza"), 
            ("STR002", "Ankara Kızılay Mağaza"), 
            ("STR003", "İzmir Alsancak Mağaza"),
            ("STR004", "Bursa Nilüfer Mağaza"),
            ("STR005", "Antalya Konyaaltı Mağaza")
        };
        var paymentMethods = new[] { "Nakit", "Kredi Kartı", "Banka Kartı", "Havale/EFT" };
        var sales = new List<NebimSaleDto>();
        var random = new Random(42);
        var products = Products;
        
        for (int dayOffset = 0; dayOffset < 60; dayOffset++)
        {
            var date = DateTime.UtcNow.Date.AddDays(-dayOffset);
            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            var dailySalesCount = isWeekend ? random.Next(40, 80) : random.Next(25, 50);
            
            for (int i = 0; i < dailySalesCount; i++)
            {
                var (storeCode, storeName) = stores[random.Next(stores.Length)];
                var receiptNo = $"FIS{date:yyyyMMdd}{storeCode}{i:D4}";
                var itemsInReceipt = random.Next(1, 6);
                
                for (int j = 0; j < itemsInReceipt; j++)
                {
                    var product = products[random.Next(products.Count)];
                    var qty = random.Next(1, 4);
                    var unitPrice = product.SalePrice;
                    var discountRate = random.NextDouble() > 0.70 ? random.Next(5, 30) / 100m : 0;
                    var discount = unitPrice * discountRate;
                    var total = (unitPrice - discount) * qty;
                    
                    sales.Add(new NebimSaleDto
                    {
                        ReceiptNumber = receiptNo,
                        SaleDate = date.AddHours(random.Next(10, 22)).AddMinutes(random.Next(0, 60)),
                        StoreCode = storeCode,
                        StoreName = storeName,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        ColorName = product.ColorName,
                        SizeName = product.SizeName,
                        Quantity = qty,
                        UnitPrice = unitPrice,
                        DiscountAmount = discount * qty,
                        TotalAmount = total,
                        VatAmount = total * 0.20m / 1.20m,
                        CurrencyCode = "TRY",
                        CustomerCode = random.NextDouble() > 0.4 ? $"CUS{random.Next(1, 101):D5}" : null,
                        SaleType = "Retail",
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)]
                    });
                }
            }
        }
        
        return sales;
    }

    private static List<NebimCustomerDto> GenerateMockCustomers()
    {
        var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Konya", "Adana", "Gaziantep" };
        var firstNames = new[] { "Ahmet", "Mehmet", "Ayşe", "Fatma", "Mustafa", "Zeynep", "Ali", "Elif", "Hüseyin", "Merve" };
        var lastNames = new[] { "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Öztürk", "Aydın", "Özdemir", "Arslan" };
        var customers = new List<NebimCustomerDto>();
        var random = new Random(42);
        
        for (int i = 1; i <= 100; i++)
        {
            var city = cities[random.Next(cities.Length)];
            var isCorporate = random.NextDouble() > 0.85;
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            
            customers.Add(new NebimCustomerDto
            {
                CustomerCode = $"CUS{i:D5}",
                CustomerName = isCorporate ? $"{lastName} Tekstil A.Ş." : $"{firstName} {lastName}",
                CustomerType = isCorporate ? "Corporate" : "Individual",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@email.com",
                Phone = $"05{random.Next(30, 60)}{random.Next(1000000, 10000000)}",
                City = city,
                District = $"{city} Merkez",
                Address = $"Atatürk Caddesi No: {random.Next(1, 200)}/{random.Next(1, 20)}",
                TaxNumber = isCorporate ? $"{random.Next(1000000000, 2000000000)}" : null,
                TotalSalesAmount = random.Next(500, 100000),
                TotalSalesCount = random.Next(1, 100),
                LastSaleDate = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30, 730)),
                IsActive = random.NextDouble() > 0.03
            });
        }
        
        return customers;
    }
}

/// <summary>
/// Mock Nebim Repository - Merkezi MockDataProvider'dan verileri kullanır
/// Development ortamında kullanılır
/// </summary>
public class MockNebimRepository : INebimRepository
{
    // Static provider'dan verileri al
    private IReadOnlyList<NebimProductDto> Products => MockDataProvider.Products;
    private IReadOnlyList<NebimStockDto> Stocks => MockDataProvider.Stocks;
    private IReadOnlyList<NebimSaleDto> Sales => MockDataProvider.Sales;
    private IReadOnlyList<NebimCustomerDto> Customers => MockDataProvider.Customers;
    
    public MockNebimRepository()
    {
        // Artık constructor'da veri oluşturmaya gerek yok
        // Tüm veriler MockDataProvider'dan geliyor
    }
    
    #region Products
    
    public Task<PagedResult<NebimProductDto>> GetProductsAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        var query = Products.AsQueryable();
        
        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(p => p.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(filter.ProductName))
            query = query.Where(p => p.ProductName.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(filter.CategoryName))
            query = query.Where(p => p.CategoryName == filter.CategoryName);
        
        if (!string.IsNullOrEmpty(filter.BrandName))
            query = query.Where(p => p.BrandName == filter.BrandName);
        
        var totalCount = query.Count();
        
        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();
        
        return Task.FromResult(PagedResult<NebimProductDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }
    
    public Task<NebimProductDto?> GetProductByCodeAsync(string productCode, CancellationToken ct = default)
    {
        var product = Products.FirstOrDefault(p => p.ProductCode.Equals(productCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }
    
    public Task<List<NebimProductDto>> SearchProductsAsync(string searchTerm, int limit = 20, CancellationToken ct = default)
    {
        var results = Products
            .Where(p => p.ProductCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
        return Task.FromResult(results);
    }
    
    public Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = Products.Select(p => p.CategoryName).Where(c => c != null).Cast<string>().Distinct().OrderBy(c => c).ToList();
        return Task.FromResult(categories);
    }
    
    public Task<List<string>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = Products.Select(p => p.BrandName).Where(b => b != null).Cast<string>().Distinct().OrderBy(b => b).ToList();
        return Task.FromResult(brands);
    }
    
    #endregion
    
    #region Stocks
    
    public Task<PagedResult<NebimStockDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        var query = Stocks.AsQueryable();
        
        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(s => s.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(filter.WarehouseCode))
            query = query.Where(s => s.WarehouseCode == filter.WarehouseCode);
        
        if (filter.MinQuantity.HasValue)
            query = query.Where(s => s.Quantity >= filter.MinQuantity.Value);
        
        if (filter.InStockOnly == true)
            query = query.Where(s => s.Quantity > 0);
        
        var totalCount = query.Count();
        
        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();
        
        return Task.FromResult(PagedResult<NebimStockDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }
    
    public Task<List<NebimStockDto>> GetStockByProductCodeAsync(string productCode, CancellationToken ct = default)
    {
        var stocks = Stocks.Where(s => s.ProductCode.Equals(productCode, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(stocks);
    }
    
    public Task<List<NebimStockDto>> GetLowStockItemsAsync(int threshold = 10, CancellationToken ct = default)
    {
        var lowStocks = Stocks.Where(s => s.Quantity > 0 && s.Quantity < threshold).OrderBy(s => s.Quantity).ToList();
        return Task.FromResult(lowStocks);
    }
    
    public Task<decimal> GetTotalStockValueAsync(CancellationToken ct = default)
    {
        // Mock: Her stok birimi ortalama 100 TL varsay
        var totalValue = Stocks.Sum(s => s.Quantity * 100m);
        return Task.FromResult(totalValue);
    }
    
    public Task<(int TotalRecords, int TotalQuantity)> GetStockSummaryAsync(CancellationToken ct = default)
    {
        var totalRecords = Stocks.Count;
        var totalQuantity = (int)Stocks.Sum(s => s.Quantity);
        return Task.FromResult((totalRecords, totalQuantity));
    }
    
    #endregion
    
    #region Sales
    
    public Task<PagedResult<NebimSaleDto>> GetSalesAsync(SalesFilterRequest filter, CancellationToken ct = default)
    {
        var query = Sales.AsQueryable();
        
        if (filter.StartDate.HasValue)
            query = query.Where(s => s.SaleDate >= filter.StartDate.Value);
        
        if (filter.EndDate.HasValue)
            query = query.Where(s => s.SaleDate <= filter.EndDate.Value);
        
        if (!string.IsNullOrEmpty(filter.StoreCode))
            query = query.Where(s => s.StoreCode == filter.StoreCode);
        
        if (!string.IsNullOrEmpty(filter.ProductCode))
            query = query.Where(s => s.ProductCode.Contains(filter.ProductCode, StringComparison.OrdinalIgnoreCase));
        
        var totalCount = query.Count();
        
        var items = query
            .OrderByDescending(s => s.SaleDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();
        
        return Task.FromResult(PagedResult<NebimSaleDto>.Create(items, filter.Page, filter.PageSize, totalCount));
    }
    
    public Task<List<NebimSaleDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10, CancellationToken ct = default)
    {
        // Ürün bazında grupla ve toplam miktar/tutar hesapla
        var topProducts = Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => s.ProductCode)
            .OrderByDescending(g => g.Sum(s => s.TotalAmount))
            .Take(limit)
            .Select(g => {
                var first = g.First();
                // Toplam miktar ve tutarı hesapla
                return new NebimSaleDto
                {
                    ProductCode = first.ProductCode,
                    ProductName = first.ProductName,
                    ColorName = first.ColorName,
                    SizeName = first.SizeName,
                    StoreCode = first.StoreCode,
                    StoreName = first.StoreName,
                    SaleDate = first.SaleDate,
                    ReceiptNumber = first.ReceiptNumber,
                    Quantity = g.Sum(s => s.Quantity),           // TOPLAM miktar
                    UnitPrice = first.UnitPrice,
                    TotalAmount = g.Sum(s => s.TotalAmount),     // TOPLAM tutar
                    DiscountAmount = g.Sum(s => s.DiscountAmount),
                    VatAmount = g.Sum(s => s.VatAmount),
                    PaymentMethod = first.PaymentMethod,
                    CustomerCode = first.CustomerCode
                };
            })
            .ToList();
        return Task.FromResult(topProducts);
    }
    
    public Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var total = Sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).Sum(s => s.TotalAmount);
        return Task.FromResult(total);
    }
    
    public Task<Dictionary<DateTime, decimal>> GetDailySalesAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var dailySales = Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => s.SaleDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalAmount));
        return Task.FromResult(dailySales);
    }
    
    #endregion
    
    #region Customers
    
    public Task<PagedResult<NebimCustomerDto>> GetCustomersAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        var query = Customers.AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.CustomerCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        
        var totalCount = query.Count();
        
        var items = query
            .OrderBy(c => c.CustomerName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return Task.FromResult(PagedResult<NebimCustomerDto>.Create(items, page, pageSize, totalCount));
    }
    
    public Task<NebimCustomerDto?> GetCustomerByCodeAsync(string customerCode, CancellationToken ct = default)
    {
        var customer = Customers.FirstOrDefault(c => c.CustomerCode.Equals(customerCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(customer);
    }
    
    public Task<List<NebimCustomerDto>> GetTopCustomersAsync(int limit = 10, CancellationToken ct = default)
    {
        var topCustomers = Customers.OrderByDescending(c => c.TotalSalesAmount).Take(limit).ToList();
        return Task.FromResult(topCustomers);
    }
    
    #endregion
    
    #region Dashboard
    
    public Task<int> GetTotalProductCountAsync(CancellationToken ct = default)
        => Task.FromResult(Products.Count);
    
    public Task<int> GetTotalCustomerCountAsync(CancellationToken ct = default)
        => Task.FromResult(Customers.Count);
    
    #endregion
}
