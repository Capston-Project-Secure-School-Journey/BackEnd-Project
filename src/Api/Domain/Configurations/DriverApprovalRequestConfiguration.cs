using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class DriverApprovalRequestConfiguration : IEntityTypeConfiguration<DriverApprovalRequest>
{
    public void Configure(EntityTypeBuilder<DriverApprovalRequest> builder)
    {
        builder.ToTable("driver_approval_requests");

        builder.HasKey(e => e.Id);

        builder.Property(u => u.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(u => u.RequestedDate)
            .HasColumnName("requested_date")
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(u => u.DriverId)
            .HasColumnName("driver_id")
            .IsRequired();

        builder.Property(u => u.RequestStatus)
            .HasColumnName("request_status")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(u => u.ApprovedBy)
            .HasColumnName("approved_by")
            .IsRequired(false);

        builder.Property(u => u.VehicleType)
            .HasColumnName("vehicle_type")
            .IsRequired()
            .HasColumnType("nvarchar(200)");

        builder.Property(u => u.LicenseNumber)
            .HasColumnName("license_number")
            .IsRequired()
            .HasColumnType("varchar(50)");

        builder.Property(u => u.DriverInformationImages)
            .HasColumnName("driver_information_images")
            .IsRequired()
            .HasColumnType("json");

        builder.Property(u => u.VehicleImages)
            .HasColumnName("vehicle_images")
            .IsRequired()
            .HasColumnType("json");

        builder.Property(u => u.LastCheckDrivingLicense)
            .HasColumnName("last_check_driving_license")
            .IsRequired(false)
            .HasColumnType("datetime");

        builder.HasMany(e => e.DriverRequestStatusHistories)
            .WithOne(x => x.Request)
            .HasForeignKey(e => e.RequestId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}