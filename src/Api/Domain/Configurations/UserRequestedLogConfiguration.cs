using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class UserRequestedLogConfiguration : IEntityTypeConfiguration<UserRequestedLog>
{
    public void Configure(EntityTypeBuilder<UserRequestedLog> builder)
    {
        builder.ToTable("user_requested_logs");

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("int unsigned")
            .UseMySqlIdentityColumn()
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(c => c.UserRequestedType)
            .HasColumnName("user_requested_type")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(c => c.DatetimeRequested)
            .HasColumnName("datetime_requested")
            .HasColumnType("datetime")
            .IsRequired();
    }
}