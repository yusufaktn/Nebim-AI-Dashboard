using Entity.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

/// <summary>
/// ChatSession entity konfigürasyonu
/// </summary>
public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        // Table
        builder.ToTable("ChatSessions");
        
        // Primary Key
        builder.HasKey(cs => cs.Id);
        
        // Properties
        builder.Property(cs => cs.Title)
            .HasMaxLength(500);
        
        builder.Property(cs => cs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(cs => cs.MessageCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        // Indexes
        builder.HasIndex(cs => cs.UserId)
            .HasDatabaseName("IX_ChatSessions_UserId");
        
        builder.HasIndex(cs => cs.IsActive)
            .HasDatabaseName("IX_ChatSessions_IsActive");
        
        builder.HasIndex(cs => cs.CreatedAt)
            .HasDatabaseName("IX_ChatSessions_CreatedAt");
        
        // Composite index for common query: user's active sessions
        builder.HasIndex(cs => new { cs.UserId, cs.IsActive })
            .HasDatabaseName("IX_ChatSessions_UserId_IsActive");
        
        // Relationships
        builder.HasMany(cs => cs.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
