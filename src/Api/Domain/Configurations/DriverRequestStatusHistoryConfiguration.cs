using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class DriverRequestStatusHistoryConfiguration : IEntityTypeConfiguration<DriverRequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<DriverRequestStatusHistory> builder)
    {
        builder.ToTable("driver_request_status_histories");

        builder.HasKey(e => e.Id);

        builder.Property(u => u.RequestId)
            .HasColumnName("request_id")
            .IsRequired();

        builder.Property(u => u.FromStatus)
            .HasColumnName("from_status")
            .HasColumnType("tinyint")
            .IsRequired(false);

        builder.Property(u => u.ToStatus)
            .HasColumnName("to_status")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(u => u.ChangedBy)
            .HasColumnName("changed_by")
            .IsRequired();

        builder.Property(u => u.ChangedAt)
            .HasColumnName("changed_at")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(u => u.Note)
            .HasColumnName("note")
            .HasColumnType("nvarchar(1000)")
            .IsRequired();
    }
}