using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// Tenant entity configuration.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.TaxNumber)
            .HasMaxLength(20);

        builder.Property(t => t.ContactEmail)
            .HasMaxLength(200);

        builder.Property(t => t.ContactPhone)
            .HasMaxLength(50);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        // Nebim connection (encrypted)
        builder.Property(t => t.NebimConnectionStringEncrypted)
            .HasMaxLength(2000);

        builder.Property(t => t.NebimServerHost)
            .HasMaxLength(200);

        builder.Property(t => t.NebimDatabaseName)
            .HasMaxLength(200);

        builder.Property(t => t.NebimLastErrorMessage)
            .HasMaxLength(1000);

        // Enum conversions
        builder.Property(t => t.NebimServerType)
            .HasConversion<int>();

        builder.Property(t => t.ConnectionStatus)
            .HasConversion<int>();

        builder.Property(t => t.OnboardingStatus)
            .HasConversion<int>();

        // Relationships
        builder.HasOne(t => t.SubscriptionPlan)
            .WithMany(sp => sp.Tenants)
            .HasForeignKey(t => t.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Users ilişkisi - TenantId nullable olduğu için IsRequired(false)
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.QueryHistories)
            .WithOne(qh => qh.Tenant)
            .HasForeignKey(qh => qh.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.QueryQuotas)
            .WithOne(qq => qq.Tenant)
            .HasForeignKey(qq => qq.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete filter
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
