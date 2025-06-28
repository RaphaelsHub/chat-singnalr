using ChatSinglar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatSinglar.Data.Configurations;

public class MessageModelConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EncryptedText).IsRequired();
        builder.Property(m => m.Timestamp).IsRequired();
        builder.HasOne<Chat>()
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => m.ChatId);
        builder.HasIndex(m => m.SenderId);
    }

}