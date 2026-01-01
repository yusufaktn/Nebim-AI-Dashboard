using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

/// <summary>
/// API servisleri için DI extension
/// 
/// 🎓 AÇIKLAMA:
/// JWT (JSON Web Token) Authentication:
/// - Stateless: Sunucuda session tutmaya gerek yok
/// - Token içinde kullanıcı bilgileri var (claims)
/// - Her request'te Authorization header'da gönderilir
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// JWT Authentication ekle
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] 
            ?? throw new InvalidOperationException("JWT Secret yapılandırılmamış!");
        
        services.AddAuthentication(options =>
        {
            // 🎓 Varsayılan scheme: JWT Bearer
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // 🎓 Token doğrulama parametreleri
                ValidateIssuer = true,           // Token'ı kim oluşturdu?
                ValidateAudience = true,         // Token kimin için?
                ValidateLifetime = true,         // Süresi dolmuş mu?
                ValidateIssuerSigningKey = true, // İmza geçerli mi?
                
                ValidIssuer = jwtSettings["Issuer"] ?? "NebimDashboard",
                ValidAudience = jwtSettings["Audience"] ?? "NebimDashboard",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),
                
                // Saat farkı toleransı
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            // 🎓 Event handlers (opsiyonel, debugging için)
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Authentication failed: {Error}", 
                        context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// CORS politikası ekle
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:5173"]; // Vite default

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Swagger/OpenAPI ekle
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Nebim Dashboard API",
                Version = "v1",
                Description = "Nebim ERP Dashboard için REST API"
            });

            // 🎓 JWT Bearer için Swagger ayarı
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT token'ınızı girin. Örnek: eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
