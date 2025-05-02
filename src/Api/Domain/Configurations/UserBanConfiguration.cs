using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class UserBanConfiguration : IEntityTypeConfiguration<UserBan>
{
    public void Configure(EntityTypeBuilder<UserBan> builder)
    {
        builder.ToTable("user_bans");

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType(Constants.IntUnsigned)
            .UseMySqlIdentityColumn()
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .HasColumnType(Constants.Char(36))
            .IsRequired();

        builder.Property(c => c.BanType)
            .HasColumnName("ban_type")
            .HasColumnType(Constants.Tinyint)
            .IsRequired();

        builder.Property(c => c.BanDate)
            .HasColumnName("ban_date")
            .HasColumnType(Constants.Datetime)
            .IsRequired();

        builder.Property(c => c.BanExpiryDate)
            .HasColumnName("ban_expiry_date")
            .HasColumnType(Constants.Datetime)
            .IsRequired();
    }
}