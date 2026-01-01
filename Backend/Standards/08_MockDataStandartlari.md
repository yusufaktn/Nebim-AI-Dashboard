# Mock Data Yönetimi Standartları

## 1. Genel Bakış

Bu doküman, Nebim Dashboard projesinde **mock/simülasyon verilerinin** nasıl yönetildiğini ve gelecekteki geliştirmelerde uyulması gereken kuralları açıklar.

## 2. Mimari

### 2.1 Merkezi Veri Sağlayıcı: `MockDataProvider`

Tüm mock veriler **tek bir kaynaktan** sağlanır:

```
MockDataProvider (Static, Lazy)
       │
       ├─> MockNebimRepository (INebimRepository)
       │         ↓
       │   AIService, StockService, DashboardService
       │
       └─> SimulatedNebimRepository (INebimRepositoryWithContext)
                 ↓
           NebimRepositoryFactory → AI Capabilities
```

**Dosya Konumu:** `DAL/Repositories/Nebim/MockNebimRepository.cs`

### 2.2 MockDataProvider Özellikleri

```csharp
public static class MockDataProvider
{
    // Lazy initialization - thread-safe, tek seferlik oluşturma
    private static readonly Lazy<List<NebimProductDto>> _products = 
        new(() => GenerateMockProducts(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    // Public readonly erişim
    public static IReadOnlyList<NebimProductDto> Products => _products.Value;
    public static IReadOnlyList<NebimStockDto> Stocks => _stocks.Value;
    public static IReadOnlyList<NebimSaleDto> Sales => _sales.Value;
    public static IReadOnlyList<NebimCustomerDto> Customers => _customers.Value;
}
```

### 2.3 Repository Sınıfları

| Sınıf | Interface | Kullanım Alanı | Veri Kaynağı |
|-------|-----------|----------------|--------------|
| `MockNebimRepository` | `INebimRepository` | Service'ler (AIService, StockService, vb.) | `MockDataProvider` |
| `SimulatedNebimRepository` | `INebimRepositoryWithContext` | Tenant-aware işlemler, Capabilities | `MockDataProvider` |
| `TenantAwareNebimRepository` | `INebimRepositoryWithContext` | Gerçek Nebim bağlantısı | SQL Server |

## 3. Önemli Kurallar

### 3.1 ✅ YAPILMASI GEREKENLER

1. **Tek Veri Kaynağı Kullan**
   - Tüm mock repository'ler `MockDataProvider`'dan veri almalı
   - Asla repository içinde kendi verisini oluşturmamalı

2. **Static Veri Üretimi**
   - Mock veriler `Lazy<T>` ile oluşturulmalı
   - Thread-safe olmalı
   - Uygulama ömrü boyunca değişmemeli

3. **Tutarlı Seed Değeri**
   - `Random(42)` kullanarak deterministik veriler üret
   - Her çalıştırmada aynı veriler oluşsun

4. **IReadOnlyList Kullan**
   - Dışarıya `IReadOnlyList<T>` olarak sun
   - Verilerin değiştirilmesini engelle

### 3.2 ❌ YAPILMAMASI GEREKENLER

1. **Repository Constructor'ında Veri Üretme**
   ```csharp
   // ❌ YANLIŞ
   public MockNebimRepository()
   {
       _products = GenerateMockProducts(); // Her instance'ta yeni veri!
   }
   
   // ✅ DOĞRU
   public MockNebimRepository()
   {
       // MockDataProvider'dan al
   }
   private IReadOnlyList<NebimProductDto> Products => MockDataProvider.Products;
   ```

2. **Tenant Bazlı Farklı Mock Veri**
   ```csharp
   // ❌ YANLIŞ - Her tenant farklı veri görür
   var seed = tenantId ?? 42;
   _products = GenerateMockProducts(seed);
   
   // ✅ DOĞRU - Tüm tenant'lar aynı veriyi görür (development'ta)
   private IReadOnlyList<NebimProductDto> Products => MockDataProvider.Products;
   ```

3. **Yeni Mock Repository Sınıfı Oluşturma**
   - Mevcut `MockNebimRepository` veya `SimulatedNebimRepository` kullanın
   - Yeni sınıf gerekiyorsa `MockDataProvider`'dan veri alın

## 4. Dependency Injection Yapılandırması

**Dosya:** `DAL/Extensions/ServiceCollectionExtensions.cs`

```csharp
// Mock repository - Singleton (development için)
services.AddSingleton<INebimRepository, MockNebimRepository>();

// Repository factory - Scoped (tenant-aware işlemler için)
services.AddScoped<INebimRepositoryFactory, NebimRepositoryFactory>();
```

## 5. Veri Tutarlılığı

### 5.1 Aynı Verileri Gören Bileşenler

| Bileşen | Veri Türü | Örnek |
|---------|-----------|-------|
| AI Asistan | En çok satan ürünler | "Zara Slim Fit Gömlek - 150 adet" |
| Satış Raporu | En çok satan ürünler | "Zara Slim Fit Gömlek - 150 adet" |
| Dashboard | Toplam stok | "309 kayıt, 15584 adet" |
| AI Asistan | Toplam stok | "309 kayıt, 15584 adet" |

### 5.2 Veri Doğrulama

Sistemin tutarlı çalışıp çalışmadığını test etmek için:

1. AI Asistan'a "en çok satan 5 ürün" sorun
2. Satış Raporu sayfasını açın
3. Aynı ürünlerin aynı sırayla görünmesi gerekir

## 6. Mock Veri Şeması

### 6.1 Ürünler (100 adet)
- 10 marka × 10 kategori
- Gerçekçi Türk markaları (Zara, H&M, Koton, vb.)
- Fiyat aralığı: 100-2500 TL

### 6.2 Stoklar (~309 kayıt)
- Her ürün 2-4 depoda
- %10 stoksuz, %20 düşük stok, %70 normal stok
- 6 depo/mağaza

### 6.3 Satışlar (~3000 kayıt)
- Son 60 gün
- Hafta sonu daha fazla satış
- 5 mağaza

### 6.4 Müşteriler (100 adet)
- %85 bireysel, %15 kurumsal
- Türk isimleri ve şehirleri

## 7. Production'a Geçiş

Production ortamında:

1. `NebimRepositoryFactory` gerçek `TenantAwareNebimRepository` döner
2. Mock veriler kullanılmaz
3. Tenant'ın Nebim SQL Server'ına bağlanılır

```csharp
// NebimRepositoryFactory.CreateAsync()
if (tenant.NebimServerType == NebimServerType.Production)
{
    return new TenantAwareNebimRepository(tenantId, connectionManager, logger);
}
else
{
    return new SimulatedNebimRepository(tenantId); // Mock veri
}
```

## 8. Yeni Veri Türü Ekleme

Yeni bir Nebim veri türü eklerken:

1. `MockDataProvider`'a yeni `Lazy<List<T>>` property ekle
2. `GenerateMock[Type]()` metodu yaz
3. Her iki repository'de (`MockNebimRepository`, `SimulatedNebimRepository`) kullan
4. `INebimRepository` interface'ine metot ekle

```csharp
// MockDataProvider'a ekle
private static readonly Lazy<List<NebimNewTypeDto>> _newTypes = 
    new(() => GenerateMockNewTypes(), LazyThreadSafetyMode.ExecutionAndPublication);

public static IReadOnlyList<NebimNewTypeDto> NewTypes => _newTypes.Value;

private static List<NebimNewTypeDto> GenerateMockNewTypes()
{
    var random = new Random(42); // Aynı seed!
    // Veri üretimi...
}
```

## 9. Sık Karşılaşılan Sorunlar

### 9.1 "Frontend ve AI farklı veri gösteriyor"

**Neden:** Farklı repository instance'ları farklı veri üretiyor
**Çözüm:** Tüm repository'lerin `MockDataProvider`'dan veri aldığını kontrol et

### 9.2 "Her yenilemede veriler değişiyor"

**Neden:** Random seed sabit değil
**Çözüm:** `new Random(42)` kullan, deterministik olsun

### 9.3 "Stok sayısı tutmuyor"

**Neden:** Bir yer pagination ile sınırlı veri alıyor
**Çözüm:** Özet için `GetStockSummaryAsync()` gibi aggregate metotlar kullan

---

**Son Güncelleme:** 28 Aralık 2025
**Versiyon:** 1.0
