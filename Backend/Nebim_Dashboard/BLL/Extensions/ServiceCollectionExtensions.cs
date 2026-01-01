using BLL.AI.Capabilities;
using BLL.AI.Capabilities.Implementations;
using BLL.AI.Orchestrator;
using BLL.AI.Planner;
using BLL.AI.Validation;
using BLL.Services;
using BLL.Services.AI;
using BLL.Services.Interfaces;
using BLL.Services.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

/// <summary>
/// BLL servisleri için DI extension
/// 
/// 🎓 AÇIKLAMA - Dependency Injection (Bağımlılık Enjeksiyonu):
/// 
/// Sorun: new UserService(new UnitOfWork(...)) şeklinde sınıf içinde bağımlılık oluşturmak
/// - Test edilemez (gerçek veritabanına bağlı)
/// - Değiştirilemez (sınıf içinde sabit)
/// - Sıkı bağlı (tight coupling)
/// 
/// Çözüm: DI Container
/// - Bağımlılıklar dışarıdan verilir
/// - Interface üzerinden çalışır
/// - Test için mock verilebilir
/// 
/// Lifetime (Yaşam Döngüsü):
/// - Singleton: Uygulama boyunca tek instance
/// - Scoped: Her HTTP request için yeni instance (DB işlemleri için ideal)
/// - Transient: Her çağrıda yeni instance
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Business Logic Layer servislerini DI'a ekle
    /// </summary>
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // 🎓 Scoped: Her HTTP request için yeni instance
        // Neden Scoped?
        // - UnitOfWork Scoped olduğu için servisler de Scoped olmalı
        // - Request boyunca aynı DbContext kullanılır
        // - Request bitince dispose edilir
        
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IStockService, StockService>();
        
        // 🤖 AI Servisi - Google Gemini 1.5 Flash (Legacy)
        services.AddScoped<IAIService, AIService>();
        
        // 🧠 AI Business Intelligence Services
        services.AddAIBusinessIntelligence();
        
        // 🏢 Tenant Services
        services.AddTenantServices();
        
        return services;
    }

    /// <summary>
    /// AI İş Zekası servislerini DI'a ekle
    /// </summary>
    private static IServiceCollection AddAIBusinessIntelligence(this IServiceCollection services)
    {
        // Capability'ler - Scoped (INebimRepositoryFactory scoped olduğu için)
        // Her request'te tenant'a özel repository kullanılır
        services.AddScoped<ICapability, GetSalesCapability>();
        services.AddScoped<ICapability, GetStockCapability>();
        services.AddScoped<ICapability, GetTopProductsCapability>();
        services.AddScoped<ICapability, GetLowStockAlertsCapability>();
        services.AddScoped<ICapability, ComparePeriodCapability>();
        services.AddScoped<ICapability, GetProductDetailsCapability>();

        // Capability Registry - Scoped (capability'leri her request'te çözer)
        services.AddScoped<ICapabilityRegistry, CapabilityRegistry>();

        // Query Planner - Scoped (HttpClient kullanır)
        services.AddScoped<IQueryPlanner, GeminiQueryPlanner>();
        services.AddHttpClient<GeminiQueryPlanner>();

        // Validators - Scoped
        services.AddScoped<IQueryPlanValidator, QueryPlanValidator>();
        services.AddScoped<ISubscriptionValidator, SubscriptionValidator>();
        services.AddScoped<ITenantValidator, TenantValidator>();

        // Orchestrator - Scoped
        services.AddScoped<IQueryOrchestrator, QueryOrchestrator>();

        // Main BI Service - Scoped
        services.AddScoped<IBusinessIntelligenceService, BusinessIntelligenceService>();

        return services;
    }

    /// <summary>
    /// Tenant servislerini DI'a ekle
    /// </summary>
    private static IServiceCollection AddTenantServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();

        return services;
    }
}
