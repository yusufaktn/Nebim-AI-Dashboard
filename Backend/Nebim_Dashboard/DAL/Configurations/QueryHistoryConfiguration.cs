using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// QueryHistory entity configuration.
/// </summary>
public class QueryHistoryConfiguration : IEntityTypeConfiguration<QueryHistory>
{
    public void Configure(EntityTypeBuilder<QueryHistory> builder)
    {
        builder.ToTable("QueryHistories");

        builder.HasKey(qh => qh.Id);

        builder.Property(qh => qh.RawQuery)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(qh => qh.ParsedPlanJson)
            .HasColumnType("jsonb");

        builder.Property(qh => qh.Intent)
            .HasConversion<int>();

        builder.Property(qh => qh.ExecutedCapabilities)
            .HasMaxLength(500);

        builder.Property(qh => qh.CapabilityVersionsJson)
            .HasColumnType("jsonb");

        builder.Property(qh => qh.ResponseSummary)
            .HasMaxLength(1000);

        builder.Property(qh => qh.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(qh => qh.DataSource)
            .HasMaxLength(50)
            .HasDefaultValue("real");

        builder.Property(qh => qh.ClientIpAddress)
            .HasMaxLength(50);

        builder.Property(qh => qh.UserAgent)
            .HasMaxLength(500);

        // Indexes for querying
        builder.HasIndex(qh => qh.TenantId);
        builder.HasIndex(qh => qh.UserId);
        builder.HasIndex(qh => qh.CreatedAt);
        builder.HasIndex(qh => new { qh.TenantId, qh.CreatedAt });

        // Relationships - User ilişkisi burada tanımlı (Tenant ilişkisi TenantConfiguration'da)
        builder.HasOne(qh => qh.User)
            .WithMany(u => u.QueryHistories)
            .HasForeignKey(qh => qh.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
