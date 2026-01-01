using DAL.Context;
using Entity.App;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Data;

/// <summary>
/// Veritabanı başlangıç verileri (Seed Data)
/// 🎓 Development ortamında test için örnek veriler oluşturur
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seed verilerini uygula
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Users tablosu boşsa seed uygula
        if (!await context.Users.AnyAsync())
        {
            await SeedUsersAsync(context);
        }
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        // 🎓 PBKDF2 ile hash'lenmiş şifreler
        
        var users = new List<User>
        {
            new()
            {
                Email = "admin@nebim.com",
                PasswordHash = HashPassword("Admin123!"),
                FullName = "Admin Kullanıcı",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Email = "user@nebim.com",
                PasswordHash = HashPassword("User123!"),
                FullName = "Test Kullanıcı",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Email = "manager@nebim.com",
                PasswordHash = HashPassword("Manager123!"),
                FullName = "Yönetici Kullanıcı",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        
        // Admin için ayarlar
        var adminSettings = new UserSetting
        {
            UserId = users[0].Id,
            Theme = "dark",
            Language = "tr",
            EmailNotifications = true,
            DashboardWidgets = "[\"sales\",\"stock\",\"alerts\"]"
        };
        
        await context.UserSettings.AddAsync(adminSettings);
        
        // Örnek chat session'ları
        var chatSessions = new List<ChatSession>
        {
            new()
            {
                UserId = users[0].Id,
                Title = "Stok Analizi",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                UserId = users[1].Id,
                Title = "Satış Raporu Hakkında",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        
        await context.ChatSessions.AddRangeAsync(chatSessions);
        await context.SaveChangesAsync();
        
        // Örnek mesajlar
        var messages = new List<ChatMessage>
        {
            new()
            {
                SessionId = chatSessions[0].Id,
                Role = ChatRole.User,
                Content = "Son bir haftanın stok durumunu analiz edebilir misin?",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(1)
            },
            new()
            {
                SessionId = chatSessions[0].Id,
                Role = ChatRole.Assistant,
                Content = "Tabii! Son 7 günün stok analizine göre:\n\n📊 **Genel Durum:**\n- Toplam ürün sayısı: 1,245\n- Kritik stok seviyesinde: 23 ürün\n- Stok devir hızı: Ortalama 4.2 gün\n\n⚠️ **Dikkat Edilmesi Gerekenler:**\n- Elektronik kategorisinde %15 azalma\n- Tekstil ürünlerinde talep artışı\n\nDetaylı rapor ister misiniz?",
                CreatedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(2)
            },
            new()
            {
                SessionId = chatSessions[1].Id,
                Role = ChatRole.User,
                Content = "Bu ayki satış rakamlarını özetle",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(5)
            },
            new()
            {
                SessionId = chatSessions[1].Id,
                Role = ChatRole.Assistant,
                Content = "📈 **Aralık 2025 Satış Özeti:**\n\n💰 **Toplam Ciro:** ₺2,450,000\n📦 **Satılan Ürün:** 3,842 adet\n👥 **Aktif Müşteri:** 567\n\n**En Çok Satan Kategoriler:**\n1. Elektronik - ₺890,000\n2. Giyim - ₺650,000\n3. Ev & Yaşam - ₺420,000\n\nGeçen aya göre %12 artış var!",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(6)
            }
        };
        
        await context.ChatMessages.AddRangeAsync(messages);
    }

    /// <summary>
    /// Basit PBKDF2 hash (Seed için)
    /// 🎓 Production'da BLL/Helpers/PasswordHelper kullanılmalı
    /// </summary>
    private static string HashPassword(string password)
    {
        using var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password,
            saltSize: 16,
            iterations: 100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256);
        
        var salt = deriveBytes.Salt;
        var hash = deriveBytes.GetBytes(32);
        
        var result = new byte[48];
        Buffer.BlockCopy(salt, 0, result, 0, 16);
        Buffer.BlockCopy(hash, 0, result, 16, 32);
        
        return Convert.ToBase64String(result);
    }
}
