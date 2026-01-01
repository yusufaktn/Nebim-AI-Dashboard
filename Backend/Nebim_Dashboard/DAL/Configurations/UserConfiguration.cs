using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// User entity konfigürasyonu
/// Index isimleri manuel belirleniyor - migration hatalarını önlemek için
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table
        builder.ToTable("Users");
        
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        // TenantId nullable - sistem kullanıcıları tenant'sız olabilir
        builder.Property(u => u.TenantId)
            .IsRequired(false);

        builder.Property(u => u.IsTenantAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(512);
        
        // Audit properties
        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);
        
        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(256);
        
        builder.Property(u => u.DeletedBy)
            .HasMaxLength(256);
        
        // Indexes - İsimler manuel belirleniyor
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
        
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");
        
        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");
        
        // Soft delete için global query filter
        builder.HasQueryFilter(u => !u.IsDeleted);
        
        // Relationships - Tenant ilişkisi TenantConfiguration'da tanımlı
        builder.HasOne(u => u.Setting)
            .WithOne(s => s.User)
            .HasForeignKey<UserSetting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(u => u.ChatSessions)
            .WithOne(cs => cs.User)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // QueryHistories ilişkisi TenantConfiguration'da tanımlı
    }
}
