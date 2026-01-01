using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// QueryQuota entity configuration.
/// </summary>
public class QueryQuotaConfiguration : IEntityTypeConfiguration<QueryQuota>
{
    public void Configure(EntityTypeBuilder<QueryQuota> builder)
    {
        builder.ToTable("QueryQuotas");

        builder.HasKey(qq => qq.Id);

        builder.Property(qq => qq.PeriodType)
            .HasConversion<int>();

        // Composite index for fast lookup
        builder.HasIndex(qq => new { qq.TenantId, qq.PeriodType, qq.PeriodStart })
            .IsUnique();

        // Relationship - Tenant configuration'da tanımlı, burada tanımlamıyoruz
    }
}
