# 📂 Dosya Referansı - AI BI Sistemi

Bu doküman, AI Business Intelligence sisteminin tüm dosyalarını ve kısa açıklamalarını içerir.

---

## 🎯 Entity Katmanı

### Entity/DTOs/AI/

| Dosya | Açıklama |
|-------|----------|
| `BusinessQueryRequest.cs` | Kullanıcının doğal dil sorgusunu frontend'den backend'e taşıyan DTO |
| `BusinessQueryResponse.cs` | Sorgu sonucunu (veri, metadata, öneriler) frontend'e döndüren DTO |
| `QueryPlanDto.cs` | AI'ın ürettiği sorgu planı - hangi capability'ler hangi sırayla çalışacak |
| `CapabilityCallDto.cs` | Tek bir capability çağrısının parametreleri ve sırası |
| `CapabilityResultDto.cs` | Capability çalıştırma sonucu - veri, süre, başarı durumu |
| `CapabilityInfoDto.cs` | Capability hakkında bilgi - ad, açıklama, parametreler |
| `SuggestedCapabilityDto.cs` | Kullanıcıya önerilen capability (kapsam dışı sorularda) |

### Entity/App/ (Multi-Tenant)

| Dosya | Açıklama |
|-------|----------|
| `Tenant.cs` | Firma entity'si - Nebim bağlantı bilgileri, onboarding durumu, subscription |
| `SubscriptionPlan.cs` | Abonelik planı - Free/Pro/Enterprise, limitler, fiyatlar |
| `QueryQuota.cs` | Sorgu kotası takibi - günlük/aylık kullanım |
| `QueryHistory.cs` | Sorgu geçmişi - audit trail, reprodüce edilebilirlik |

### Entity/Enums/

| Dosya | Açıklama |
|-------|----------|
| `QueryIntent.cs` | Sorgu amacı: Descriptive, Diagnostic, Comparative, Predictive, OutOfScope |
| `SubscriptionTier.cs` | Abonelik seviyesi: Free, Professional, Enterprise |
| `OnboardingStatus.cs` | Onboarding durumu: NotStarted → Completed |
| `NebimConnectionStatus.cs` | Nebim bağlantı durumu: NotConfigured, Pending, Connected, Failed |
| `NebimServerType.cs` | Veri kaynağı: Simulation veya Production |

---

## 🧠 BLL/AI Katmanı

### BLL/AI/Capabilities/

| Dosya | Açıklama |
|-------|----------|
| `ICapability.cs` | Capability interface - tüm yeteneklerin implement ettiği sözleşme |
| `BaseCapability.cs` | Ortak capability mantığı - logging, error handling, timing |
| `CapabilityRegistry.cs` | Capability'leri isim/versiyon ile bulan DI-friendly registry |

### BLL/AI/Capabilities/Implementations/

| Dosya | Açıklama |
|-------|----------|
| `GetSalesCapability.cs` | Satış verilerini çeken capability - tarih aralığı, mağaza filtresi |
| `GetStockCapability.cs` | Stok verilerini çeken capability - depo, ürün filtresi |
| `GetTopProductsCapability.cs` | En çok satan ürünleri listeleyen capability |
| `GetLowStockAlertsCapability.cs` | Düşük stok uyarılarını döndüren capability |
| `ComparePeriodCapability.cs` | İki dönemi karşılaştıran capability - bu ay vs geçen ay |
| `GetProductDetailsCapability.cs` | Ürün detaylarını getiren capability |

### BLL/AI/Planner/

| Dosya | Açıklama |
|-------|----------|
| `IQueryPlanner.cs` | Query planner interface - sorguyu plan'a çevirir |
| `GeminiQueryPlanner.cs` | Google Gemini 2.0 Flash implementasyonu - doğal dil → JSON plan |

### BLL/AI/Validation/

| Dosya | Açıklama |
|-------|----------|
| `IValidators.cs` | Validator interface'leri - Tenant, Subscription, QueryPlan |
| `TenantValidator.cs` | Tenant kontrolü - aktif mi, Nebim bağlı mı |
| `SubscriptionValidator.cs` | Kota kontrolü - limit aşıldı mı, erişim var mı |
| `QueryPlanValidator.cs` | Plan geçerliliği - capability'ler var mı, döngü yok mu |

### BLL/AI/Orchestrator/

| Dosya | Açıklama |
|-------|----------|
| `IQueryOrchestrator.cs` | Orchestrator interface - capability'leri yönetir |
| `QueryOrchestrator.cs` | Dependency-aware capability execution - sıralama, bağımlılık çözme |

---

## 🔧 BLL/Services Katmanı

### BLL/Services/AI/

| Dosya | Açıklama |
|-------|----------|
| `IBusinessIntelligenceService.cs` | Ana BI service interface |
| `BusinessIntelligenceService.cs` | Tüm akışı yöneten ana servis - validate → plan → execute → log |

### BLL/Services/Tenant/

| Dosya | Açıklama |
|-------|----------|
| `ITenantService.cs` | Tenant CRUD interface |
| `TenantService.cs` | Tenant yönetimi - oluşturma, güncelleme, silme, arama |
| `ITenantOnboardingService.cs` | Self-service onboarding interface |
| `TenantOnboardingService.cs` | Nebim bağlantı yapılandırma, test, simulation/production geçişi |

---

## 🌐 API Katmanı

### Api/Controllers/

| Dosya | Açıklama |
|-------|----------|
| `BusinessIntelligenceController.cs` | `/api/bi/*` - Ana sorgu endpoint'i, capability listesi, geçmiş |
| `TenantOnboardingController.cs` | `/api/onboarding/*` - Self-service Nebim yapılandırma |
| `BaseController.cs` | Tüm controller'ların türediği base - Success/Error helper'ları |

### Api/Middleware/

| Dosya | Açıklama |
|-------|----------|
| `TenantResolutionMiddleware.cs` | JWT token'dan tenant_id claim'ini çözer, TenantContext'e set eder |
| `RateLimitingMiddleware.cs` | Tier bazlı rate limiting - in-memory sliding window |

---

## 📊 DAL Katmanı

### DAL/Context/

| Dosya | Açıklama |
|-------|----------|
| `ITenantContext.cs` | Request-scoped tenant bilgisi interface |
| `TenantContext.cs` | Mevcut request'in tenant/user bilgisini tutar |
| `TenantConnectionManager.cs` | Tenant'a özel Nebim connection string yönetimi (AES-256) |

### DAL/Configurations/

| Dosya | Açıklama |
|-------|----------|
| `TenantConfiguration.cs` | Tenant entity EF Core mapping - indexler, ilişkiler |
| `SubscriptionPlanConfiguration.cs` | SubscriptionPlan mapping + seed data (Free, Pro, Enterprise) |
| `QueryQuotaConfiguration.cs` | QueryQuota mapping - composite index |
| `QueryHistoryConfiguration.cs` | QueryHistory mapping - performans indexleri |
| `UserConfiguration.cs` | User mapping - TenantId, IsTenantAdmin eklendi |

### DAL/Repositories/Nebim/

| Dosya | Açıklama |
|-------|----------|
| `NebimRepositoryFactory.cs` | Tenant'a göre doğru Nebim repository'yi döndürür |
| `SimulatedNebimRepository.cs` | Fake data döndüren test repository |
| `TenantAwareNebimRepository.cs` | Gerçek Nebim V3 SQL Server'a bağlanan repository |

---

## 📋 Kullanım Akışı

```
1. Kullanıcı: "Geçen ay en çok satan 10 ürün"
                    ↓
2. BusinessIntelligenceController.Query()
                    ↓
3. TenantValidator: Tenant aktif mi?
                    ↓
4. SubscriptionValidator: Kota var mı?
                    ↓
5. GeminiQueryPlanner: Sorgu → QueryPlan (JSON)
                    ↓
6. QueryPlanValidator: Plan geçerli mi?
                    ↓
7. QueryOrchestrator: GetTopProductsCapability.Execute()
                    ↓
8. NebimRepository: SQL → Gerçek veri
                    ↓
9. QueryHistory: Audit kaydı
                    ↓
10. BusinessQueryResponse → Frontend
```

---

## 🔗 İlişkili Dokümanlar

- [AI_ARCHITECTURE.md](AI_ARCHITECTURE.md) - Detaylı mimari açıklaması
- [CHANGELOG.md](CHANGELOG.md) - Tüm değişiklikler
- [../Standards/](../Standards/) - Kodlama standartları

