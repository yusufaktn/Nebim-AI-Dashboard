# 🧠 AI Business Intelligence Mimarisi

## Genel Bakış

Bu doküman, Nebim Dashboard'un AI-powered Business Intelligence sisteminin mimarisini açıklar.

### Temel Felsefe

```
┌─────────────────────────────────────────────────────────────────┐
│  "AI cevap VERMEZ, AI yorum YAPMAZ, AI bilgi UYDURMAZ"          │
│  "AI sadece ÇEVİRMEN + PLANLAMACIDIR"                           │
│                                                                  │
│  Kullanıcı: "Geçen ayki satışları göster"                       │
│  AI: { capability: "get_sales", params: { period: "last_month" }}│
│  Backend: SQL çalıştırır → gerçek veri döner                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 Dosya Yapısı ve Açıklamalar

### 1️⃣ Entity/DTOs/AI - Veri Transfer Nesneleri

```
Entity/DTOs/AI/
├── BusinessQueryRequest.cs   # Frontend'den gelen sorgu
├── BusinessQueryResponse.cs  # Backend'den dönen sonuç
├── QueryPlanDto.cs           # AI'ın ürettiği sorgu planı
├── CapabilityCallDto.cs      # Tek capability çağrısı
├── CapabilityResultDto.cs    # Capability sonucu
├── CapabilityInfoDto.cs      # Capability hakkında bilgi
└── SuggestedCapabilityDto.cs # Kullanıcıya öneri
```

| Dosya | Ne İşe Yarar |
|-------|--------------|
| **BusinessQueryRequest** | Kullanıcının doğal dil sorgusunu taşır. `Query`, `TenantId`, `UserId` içerir |
| **BusinessQueryResponse** | Sonuç + metadata. `Data`, `QueryPlan`, `ExecutionTimeMs`, `Suggestions` içerir |
| **QueryPlanDto** | AI'ın sorguyu çevirmesiyle oluşan plan. `Intent`, `Confidence`, `Capabilities[]` |
| **CapabilityCallDto** | Hangi capability hangi parametrelerle çağrılacak. `CapabilityId`, `Parameters`, `Order` |
| **CapabilityResultDto** | Capability çalıştırma sonucu. `Data`, `RecordCount`, `IsSuccess`, `ExecutionTimeMs` |

---

### 2️⃣ BLL/AI/Capabilities - Yetenekler

```
BLL/AI/Capabilities/
├── ICapability.cs            # Tüm capability'lerin interface'i
├── BaseCapability.cs         # Ortak mantık (logging, error handling)
├── CapabilityRegistry.cs     # Capability'leri ID ile bulan registry
└── Implementations/
    ├── GetSalesCapability.cs       # Satış verilerini çeker
    ├── GetStockCapability.cs       # Stok verilerini çeker
    ├── GetTopProductsCapability.cs # En çok satan ürünleri listeler
    ├── GetLowStockAlertsCapability.cs # Düşük stok uyarıları
    ├── ComparePeriodCapability.cs  # İki dönemi karşılaştırır
    └── GetProductDetailsCapability.cs # Ürün detayları
```

#### ICapability Interface

```csharp
public interface ICapability
{
    // Benzersiz tanımlayıcı: "get_sales", "get_stock"
    string Id { get; }
    
    // Görüntüleme adı: "Satış Raporu"
    string DisplayName { get; }
    
    // Açıklama: "Belirtilen dönem için satış verilerini getirir"
    string Description { get; }
    
    // Versiyon: "1.0.0" - ileride değişirse eski sorguları reprodüce edebiliriz
    string Version { get; }
    
    // Bu capability başka capability'lere mi bağlı?
    IReadOnlyList<string> Dependencies { get; }
    
    // Çalıştır
    Task<CapabilityResultDto> ExecuteAsync(
        int tenantId, 
        Dictionary<string, object> parameters,
        CancellationToken ct = default);
}
```

#### Örnek Capability: GetSalesCapability

```csharp
public class GetSalesCapability : BaseCapability
{
    public override string Id => "get_sales";
    public override string DisplayName => "Satış Raporu";
    public override string Version => "1.0.0";
    
    protected override async Task<object> ExecuteCoreAsync(
        int tenantId, 
        Dictionary<string, object> parameters,
        CancellationToken ct)
    {
        // 1. Parametreleri parse et
        var startDate = GetDateParam(parameters, "start_date");
        var endDate = GetDateParam(parameters, "end_date");
        
        // 2. Nebim'den veri çek (tenant'a özel bağlantı)
        var sales = await _nebimRepository.GetSalesAsync(startDate, endDate, ct);
        
        // 3. Sonucu döndür (AI DEĞİL, GERÇEK VERİ)
        return sales;
    }
}
```

---

### 3️⃣ BLL/AI/Planner - Sorgu Planlayıcı

```
BLL/AI/Planner/
├── IQueryPlanner.cs          # Planner interface
└── GeminiQueryPlanner.cs     # Google Gemini implementasyonu
```

#### GeminiQueryPlanner Ne Yapar?

1. Kullanıcının doğal dil sorgusunu alır
2. Gemini 2.0 Flash API'ye gönderir
3. Hangi capability'lerin hangi sırayla çağrılacağını JSON olarak alır
4. QueryPlanDto olarak döndürür

```csharp
// Kullanıcı: "Geçen ay en çok satan 10 ürünü ve stok durumlarını göster"
// Gemini'nin döndürdüğü plan:
{
    "intent": "Descriptive",
    "confidence": 0.95,
    "capabilities": [
        {
            "capabilityId": "get_top_products",
            "parameters": { "period": "last_month", "limit": 10 },
            "order": 1
        },
        {
            "capabilityId": "get_stock",
            "parameters": { "product_ids": "@previous_result.product_ids" },
            "order": 2,
            "dependsOn": ["get_top_products"]
        }
    ]
}
```

---

### 4️⃣ BLL/AI/Validation - Doğrulama

```
BLL/AI/Validation/
├── IValidators.cs            # Interface'ler
├── QueryPlanValidator.cs     # Plan geçerliliği
├── SubscriptionValidator.cs  # Kota kontrolü
└── TenantValidator.cs        # Tenant kontrolü
```

| Validator | Ne Kontrol Eder |
|-----------|-----------------|
| **TenantValidator** | Tenant aktif mi? Nebim bağlantısı var mı? |
| **SubscriptionValidator** | Günlük kota aşıldı mı? Bu capability'ye erişim var mı? |
| **QueryPlanValidator** | Plan geçerli mi? Capability'ler mevcut mu? Döngü var mı? |

---

### 5️⃣ BLL/AI/Orchestrator - Orkestratör

```
BLL/AI/Orchestrator/
├── IQueryOrchestrator.cs     # Interface
└── QueryOrchestrator.cs      # Implementasyon
```

#### QueryOrchestrator Ne Yapar?

Tüm akışı yönetir:

```
1. QueryPlan'ı al
2. Capability'leri Order'a göre sırala
3. Her capability için:
   a. Bağımlılıkları kontrol et (önceki sonuçlar hazır mı?)
   b. Parametreleri çöz (@previous_result.xxx gibi referansları değiştir)
   c. Capability'yi çalıştır
   d. Sonucu sakla
4. Tüm sonuçları birleştir ve döndür
```

---

### 6️⃣ BLL/Services/AI - Ana Servis

```
BLL/Services/AI/
├── IBusinessIntelligenceService.cs  # Interface
└── BusinessIntelligenceService.cs   # Implementasyon
```

#### BusinessIntelligenceService Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│                    KULLANICI SORGUSU                            │
│         "Geçen ay en çok satan ürünler neler?"                  │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  1. TenantValidator: Tenant aktif mi? Nebim bağlı mı?           │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. SubscriptionValidator: Kota var mı? Erişim izni var mı?    │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. QueryPlanner (Gemini): Sorguyu QueryPlan'a çevir            │
│     → { capability: "get_top_products", params: {...} }         │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. QueryPlanValidator: Plan geçerli mi? Capability var mı?     │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. QueryOrchestrator: Capability'leri sırayla çalıştır         │
│     → GetTopProductsCapability.ExecuteAsync()                   │
│     → SQL: SELECT TOP 10... FROM NebimV3.Sales                  │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  6. QueryHistory'ye kaydet (audit trail)                        │
└─────────────────────────┬───────────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│  7. BusinessQueryResponse döndür                                │
│     → { data: [...], queryPlan: {...}, suggestions: [...] }     │
└─────────────────────────────────────────────────────────────────┘
```

---

### 7️⃣ BLL/Services/Tenant - Tenant Yönetimi

```
BLL/Services/Tenant/
├── ITenantService.cs             # Tenant CRUD interface
├── TenantService.cs              # Tenant yönetimi
├── ITenantOnboardingService.cs   # Self-service onboarding interface
└── TenantOnboardingService.cs    # Nebim bağlantı yapılandırma
```

| Servis | Ne İşe Yarar |
|--------|--------------|
| **TenantService** | Tenant CRUD, arama, listeleme |
| **TenantOnboardingService** | Yeni firma kayıt, Nebim bağlantı testi, simulation/production geçişi |

---

### 8️⃣ API/Controllers - Endpoint'ler

```
Api/Controllers/
├── BusinessIntelligenceController.cs  # /api/bi/query
└── TenantOnboardingController.cs      # /api/onboarding/*
```

#### BusinessIntelligenceController Endpoint'leri

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/bi/query` | Ana sorgu endpoint'i |
| GET | `/api/bi/capabilities` | Mevcut capability listesi |
| GET | `/api/bi/history` | Sorgu geçmişi |

#### TenantOnboardingController Endpoint'leri

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/onboarding/status` | Onboarding durumu |
| POST | `/api/onboarding/nebim/configure` | Nebim bağlantı yapılandırma |
| POST | `/api/onboarding/nebim/test` | Bağlantı testi |
| POST | `/api/onboarding/simulation/enable` | Simulation modu aç |
| POST | `/api/onboarding/production/enable` | Production modu aç |
| POST | `/api/onboarding/complete` | Onboarding'i tamamla |

---

### 9️⃣ API/Middleware - Ara Katmanlar

```
Api/Middleware/
├── TenantResolutionMiddleware.cs  # JWT'den tenant çöz
└── RateLimitingMiddleware.cs      # Rate limiting
```

| Middleware | Ne İşe Yarar |
|------------|--------------|
| **TenantResolutionMiddleware** | JWT token'dan `tenant_id` claim'ini alır, TenantContext'e set eder |
| **RateLimitingMiddleware** | Tier bazlı rate limiting (Free: 10/dk, Pro: 30/dk, Enterprise: 100/dk) |

---

## 🔐 Multi-Tenant Güvenlik

### Tenant İzolasyonu

```csharp
// Her tenant kendi Nebim'ine bağlanır
// Connection string AES-256 ile şifrelenir
public class TenantConnectionManager
{
    public SqlConnection GetConnection(int tenantId)
    {
        // 1. Tenant'ın şifreli connection string'ini al
        // 2. AES-256 ile çöz
        // 3. SqlConnection oluştur ve döndür
    }
}
```

### Subscription Tier'ları

| Tier | Günlük Limit | Rate Limit | Gerçek Nebim | Capability Kısıtı |
|------|--------------|------------|--------------|-------------------|
| Free | 10 | 10/dk | ❌ Sadece Simulation | Temel capability'ler |
| Professional | 100 | 30/dk | ✅ | Tüm capability'ler |
| Enterprise | ∞ | 100/dk | ✅ | Tüm capability'ler + Özel |

---

## 📊 Veri Akışı Diyagramı

```
┌──────────┐     ┌──────────┐     ┌──────────────┐
│  React   │────▶│  API     │────▶│  BI Service  │
│  Frontend│     │  Layer   │     │              │
└──────────┘     └──────────┘     └──────┬───────┘
                                         │
                      ┌──────────────────┼──────────────────┐
                      ▼                  ▼                  ▼
               ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
               │  Validators │   │  Planner    │   │ Orchestrator│
               │             │   │  (Gemini)   │   │             │
               └─────────────┘   └─────────────┘   └──────┬──────┘
                                                          │
                                        ┌─────────────────┼─────────────────┐
                                        ▼                 ▼                 ▼
                                 ┌────────────┐   ┌────────────┐   ┌────────────┐
                                 │ Capability │   │ Capability │   │ Capability │
                                 │ GetSales   │   │ GetStock   │   │ TopProducts│
                                 └─────┬──────┘   └─────┬──────┘   └─────┬──────┘
                                       │                │                │
                                       └────────────────┼────────────────┘
                                                        ▼
                                              ┌─────────────────┐
                                              │  Nebim V3 DB    │
                                              │  (SQL Server)   │
                                              └─────────────────┘
```

---

## ✅ Sonuç

Bu mimari şunları sağlar:

1. **Güvenlik**: AI asla veri uydurmaz, sadece SQL çevirir
2. **İzlenebilirlik**: Her sorgu QueryHistory'de loglanır
3. **Ölçeklenebilirlik**: Yeni capability eklemek kolay
4. **Multi-Tenant**: Her firma kendi Nebim'ine bağlanır
5. **Kota Yönetimi**: Subscription bazlı limitler
6. **Versiyonlama**: Capability versiyonları ile eski sorguları reprodüce edebilme

