using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// SubscriptionPlan entity configuration.
/// </summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sp => sp.Description)
            .HasMaxLength(500);

        builder.Property(sp => sp.Tier)
            .HasConversion<int>();

        builder.Property(sp => sp.AllowedCapabilitiesJson)
            .HasColumnType("jsonb");

        builder.Property(sp => sp.FeaturesJson)
            .HasColumnType("jsonb");

        builder.Property(sp => sp.PriceMonthly)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.PriceYearly)
            .HasPrecision(18, 2);

        // Seed data - CreatedAt eklendi (EF Core 9 zorunlu)
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        builder.HasData(
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Free",
                Description = "Ücretsiz başlangıç planı. Simulation modu ile temel özellikleri deneyin.",
                Tier = Entity.Enums.SubscriptionTier.Free,
                DailyQueryLimit = 10,
                MonthlyQueryLimit = 300,
                MaxConcurrentQueries = 1,
                AllowRealNebimConnection = false,
                MaxUsers = 2,
                QueryHistoryRetentionDays = 7,
                PriceMonthly = 0,
                IsActive = true,
                SortOrder = 1,
                FeaturesJson = "[\"Günlük 10 sorgu\",\"Temel raporlar\",\"Simulation modu\",\"2 kullanıcı\"]",
                CreatedAt = seedDate
            },
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Professional",
                Description = "Profesyonel kullanım için. Gerçek Nebim bağlantısı ve tüm özellikler.",
                Tier = Entity.Enums.SubscriptionTier.Professional,
                DailyQueryLimit = 100,
                MonthlyQueryLimit = 3000,
                MaxConcurrentQueries = 3,
                AllowRealNebimConnection = true,
                MaxUsers = 10,
                QueryHistoryRetentionDays = 30,
                PriceMonthly = 999,
                PriceYearly = 9990,
                IsActive = true,
                SortOrder = 2,
                FeaturesJson = "[\"Günlük 100 sorgu\",\"Tüm raporlar\",\"Gerçek Nebim bağlantısı\",\"10 kullanıcı\",\"Öncelikli destek\"]",
                CreatedAt = seedDate
            },
            new SubscriptionPlan
            {
                Id = 3,
                Name = "Enterprise",
                Description = "Kurumsal çözümler. Sınırsız kullanım ve özel özellikler.",
                Tier = Entity.Enums.SubscriptionTier.Enterprise,
                DailyQueryLimit = 0,
                MonthlyQueryLimit = 0,
                MaxConcurrentQueries = 10,
                AllowRealNebimConnection = true,
                MaxUsers = 0,
                QueryHistoryRetentionDays = 365,
                PriceMonthly = 2999,
                PriceYearly = 29990,
                IsActive = true,
                SortOrder = 3,
                FeaturesJson = "[\"Sınırsız sorgu\",\"Tüm raporlar\",\"Gerçek Nebim bağlantısı\",\"Sınırsız kullanıcı\",\"7/24 destek\",\"Özel entegrasyonlar\"]",
                CreatedAt = seedDate
            }
        );
    }
}
