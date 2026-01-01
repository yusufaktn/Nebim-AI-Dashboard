using Api.Extensions;
using Api.Middleware;
using BLL.Extensions;
using DAL.Extensions;

var builder = WebApplication.CreateBuilder(args);

// === Services ===

// DAL - Database & Repository
builder.Services.AddDataAccessLayer(builder.Configuration);

// BLL - Business Logic (Services)
builder.Services.AddBusinessLogicLayer();

// API - Authentication, CORS, Swagger
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// === Middleware Pipeline ===
// 🎓 Sıralama önemli! Exception → CORS → Auth → Controllers

// 1. Global Exception Handler (en dışta)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nebim Dashboard API v1");
        options.RoutePrefix = string.Empty; // Swagger ana sayfada açılsın
    });
    
    // Migration'ları otomatik uygula (Development)
    await app.Services.ApplyMigrationsAsync();
}

// 3. HTTPS yönlendirme
app.UseHttpsRedirection();

// 4. CORS
app.UseCors("AllowFrontend");

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Controllers
app.MapControllers();

app.Run();
