using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// ChatMessage entity konfigürasyonu
/// </summary>
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        // Table
        builder.ToTable("ChatMessages");
        
        // Primary Key
        builder.HasKey(cm => cm.Id);
        
        // Properties
        builder.Property(cm => cm.Content)
            .IsRequired()
            .HasColumnType("text"); // Uzun mesajlar için
        
        builder.Property(cm => cm.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(cm => cm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(cm => cm.Metadata)
            .HasColumnType("jsonb"); // PostgreSQL JSON tipi
        
        // Indexes
        builder.HasIndex(cm => cm.SessionId)
            .HasDatabaseName("IX_ChatMessages_SessionId");
        
        builder.HasIndex(cm => cm.CreatedAt)
            .HasDatabaseName("IX_ChatMessages_CreatedAt");
        
        builder.HasIndex(cm => cm.Role)
            .HasDatabaseName("IX_ChatMessages_Role");
        
        // Composite index for getting session messages ordered
        builder.HasIndex(cm => new { cm.SessionId, cm.CreatedAt })
            .HasDatabaseName("IX_ChatMessages_SessionId_CreatedAt");
    }
}
