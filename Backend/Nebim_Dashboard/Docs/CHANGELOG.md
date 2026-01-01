# Changelog

Tüm önemli değişiklikler bu dosyada belgelenecektir.

Format [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standardına uygundur.

## [Unreleased]

### Added - 2024-12-28

#### AI Business Intelligence System
- **Entity Layer**
  - `Entity/Enums/`: QueryIntent, NebimConnectionStatus, NebimServerType, OnboardingStatus, SubscriptionTier
  - `Entity/DTOs/AI/`: QueryPlanDto, CapabilityCallDto, CapabilityResultDto, SuggestedCapabilityDto, BusinessQueryRequest, BusinessQueryResponse, CapabilityInfoDto
  - `Entity/App/`: Tenant, SubscriptionPlan, QueryQuota, QueryHistory modelleri
  - `Entity/App/User.cs`: TenantId ve IsTenantAdmin property'leri eklendi

#### Multi-Tenant Infrastructure
- **DAL Layer**
  - `DAL/Context/TenantContext.cs`: Request-scoped tenant context
  - `DAL/Context/TenantConnectionManager.cs`: AES-256 şifreli Nebim bağlantı yönetimi
  - `DAL/Providers/NebimRepositoryFactory.cs`: Tenant-aware repository factory
  - `DAL/Repositories/Nebim/SimulatedNebimRepository.cs`: Simulation modu
  - `DAL/Repositories/Nebim/TenantAwareNebimRepository.cs`: Gerçek Nebim V3 bağlantısı
  - `DAL/Configurations/`: Tenant, SubscriptionPlan, QueryQuota, QueryHistory EF Core konfigürasyonları

#### AI Capabilities System
- **BLL Layer - Capabilities**
  - `BLL/AI/Capabilities/ICapability.cs`: Capability interface
  - `BLL/AI/Capabilities/BaseCapability.cs`: Abstract base class
  - `BLL/AI/Capabilities/CapabilityRegistry.cs`: Capability yönetimi ve versiyonlama
  - `BLL/AI/Capabilities/Implementations/`: GetSales, GetStock, GetTopProducts, GetLowStockAlerts, ComparePeriod, GetProductDetails

- **BLL Layer - Planner**
  - `BLL/AI/Planner/IQueryPlanner.cs`: Query planner interface
  - `BLL/AI/Planner/GeminiQueryPlanner.cs`: Google Gemini 2.0 Flash entegrasyonu

- **BLL Layer - Validation**
  - `BLL/AI/Validation/IValidators.cs`: Validation interface'leri
  - `BLL/AI/Validation/QueryPlanValidator.cs`: Query plan doğrulama
  - `BLL/AI/Validation/SubscriptionValidator.cs`: Kota kontrolü
  - `BLL/AI/Validation/TenantValidator.cs`: Tenant doğrulama

- **BLL Layer - Orchestrator**
  - `BLL/AI/Orchestrator/IQueryOrchestrator.cs`: Orchestrator interface
  - `BLL/AI/Orchestrator/QueryOrchestrator.cs`: Dependency-aware capability execution

- **BLL Layer - Services**
  - `BLL/Services/AI/IBusinessIntelligenceService.cs`: Ana BI service interface
  - `BLL/Services/AI/BusinessIntelligenceService.cs`: Tam pipeline implementasyonu
  - `BLL/Services/Tenant/ITenantService.cs`: Tenant yönetim interface
  - `BLL/Services/Tenant/TenantService.cs`: Tenant CRUD operasyonları
  - `BLL/Services/Tenant/ITenantOnboardingService.cs`: Self-service onboarding interface
  - `BLL/Services/Tenant/TenantOnboardingService.cs`: Nebim bağlantı yapılandırması

#### API Layer
- **Controllers**
  - `Api/Controllers/BusinessIntelligenceController.cs`: /api/bi/query, /api/bi/capabilities, /api/bi/history
  - `Api/Controllers/TenantOnboardingController.cs`: Self-service Nebim yapılandırması

- **Middleware**
  - `Api/Middleware/TenantResolutionMiddleware.cs`: JWT'den tenant çözümleme
  - `Api/Middleware/RateLimitingMiddleware.cs`: Tier-based rate limiting

### Changed
- `BLL/Extensions/ServiceCollectionExtensions.cs`: AI BI ve Tenant servisleri için DI genişletildi
- `DAL/Context/TenantContext.cs`: UserId, TenantName, SubscriptionTier property'leri eklendi

---

## [0.1.0] - 2024-12-XX (İlk Versiyon)

### Added
- Temel proje yapısı
- User authentication
- Dashboard servisleri
- Chat servisleri (legacy AI)
- Stock ve Sales servisleri
