using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DAL.Context;

/// <summary>
/// EF Core migration'ları için design-time factory
/// dotnet ef migrations add ... komutu için gerekli
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Api projesindeki appsettings.json'ı oku
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Api");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        
        var connectionString = configuration.GetConnectionString("AppDb");
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });
        
        return new AppDbContext(optionsBuilder.Options);
    }
}
