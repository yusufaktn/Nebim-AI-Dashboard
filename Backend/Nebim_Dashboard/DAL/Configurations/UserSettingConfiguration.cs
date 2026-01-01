using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// UserSetting entity konfigürasyonu
/// </summary>
public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> builder)
    {
        // Table
        builder.ToTable("UserSettings");
        
        // Primary Key
        builder.HasKey(us => us.Id);
        
        // Properties
        builder.Property(us => us.Theme)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("system");
        
        builder.Property(us => us.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("tr");
        
        builder.Property(us => us.DefaultPageSize)
            .IsRequired()
            .HasDefaultValue(20);
        
        builder.Property(us => us.DashboardWidgets)
            .HasColumnType("jsonb"); // PostgreSQL JSON tipi
        
        builder.Property(us => us.EmailNotifications)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(us => us.SaveChatHistory)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes - UserId unique olmalı (1-1 ilişki)
        builder.HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserSettings_UserId");
    }
}
