using DAL.Context;
using DAL.Providers;
using DAL.Repositories;
using DAL.Repositories.Nebim;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Extensions;

/// <summary>
/// DAL servisleri için DI extension
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Data Access Layer servislerini DI'a ekle
    /// </summary>
    public static IServiceCollection AddDataAccessLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL DbContext
        var connectionString = configuration.GetConnectionString("AppDb");
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
            
            // Development ortamında detaylı loglama
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });
        
        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        // Tenant Context (scoped - her request için ayrı)
        services.AddScoped<ITenantContext, TenantContext>();
        
        // Tenant Connection Manager
        services.AddScoped<ITenantConnectionManager, TenantConnectionManager>();
        
        // Nebim Repository Factory (tenant-aware)
        services.AddScoped<INebimRepositoryFactory, NebimRepositoryFactory>();
        
        // Legacy: Singleton mock repository (geriye uyumluluk için)
        // Yeni kodda INebimRepositoryFactory kullanılmalı
        services.AddSingleton<INebimRepository, MockNebimRepository>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, DAL.UnitOfWork.UnitOfWork>();
        
        return services;
    }
    
    /// <summary>
    /// Veritabanı migration'larını uygula
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        
        // Seed data uygula
        await Data.DbSeeder.SeedAsync(serviceProvider);
    }
    
    /// <summary>
    /// Veritabanı bağlantısını test et
    /// </summary>
    public static async Task<bool> TestDatabaseConnectionAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Database.CanConnectAsync();
    }
}
