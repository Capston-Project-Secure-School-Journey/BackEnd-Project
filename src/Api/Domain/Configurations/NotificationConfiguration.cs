using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasColumnType(Constants.Nvarchar(200))
            .IsRequired();

        builder.Property(e => e.Content)
            .HasColumnName("content")
            .HasColumnType(Constants.Nvarchar(1000))
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasColumnType(Constants.Tinyint);

        builder.Property(e => e.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.Property(e => e.SenderId)
            .HasColumnName("sender_id")
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt")
            .HasColumnType(Constants.Datetime)
            .IsRequired();

        builder.Property(e => e.IsRead)
            .HasColumnName("is_read")
            .HasColumnType(Constants.Bit)
            .IsRequired();

        builder.Property(e => e.Navigation)
            .HasColumnName("navigation")
            .HasColumnType(Constants.Varchar(300))
            .IsRequired(false);

        builder.Property(e => e.Priority)
            .HasColumnName("priority")
            .HasColumnType(Constants.Tinyint);

        builder.HasOne(e => e.Recipient)
            .WithMany()
            .HasForeignKey(e => e.RecipientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
