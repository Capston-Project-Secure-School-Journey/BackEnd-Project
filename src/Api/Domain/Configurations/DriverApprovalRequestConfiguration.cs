using Api.Common.Utilities;
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
            .HasColumnType(Constants.Datetime)
            .IsRequired();

        builder.Property(u => u.DriverId)
            .HasColumnName("driver_id")
            .IsRequired();

        builder.Property(u => u.RequestStatus)
            .HasColumnName("request_status")
            .HasColumnType(Constants.Tinyint)
            .IsRequired();

        builder.Property(u => u.ApprovedBy)
            .HasColumnName("approved_by")
            .IsRequired(false);

        builder.Property(u => u.VehicleType)
            .HasColumnName("vehicle_type")
            .IsRequired()
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(u => u.LicenseNumber)
            .HasColumnName("license_number")
            .IsRequired()
            .HasColumnType(Constants.Varchar(50));

        builder.Property(u => u.DriverInformationImages)
            .HasColumnName("driver_information_images")
            .IsRequired()
            .HasColumnType(Constants.Json);

        builder.Property(u => u.VehicleImages)
            .HasColumnName("vehicle_images")
            .IsRequired()
            .HasColumnType(Constants.Json);

        builder.Property(u => u.LastCheckDrivingLicense)
            .HasColumnName("last_check_driving_license")
            .IsRequired(false)
            .HasColumnType(Constants.Datetime);

        builder.HasMany(e => e.DriverRequestStatusHistories)
            .WithOne(x => x.Request)
            .HasForeignKey(e => e.RequestId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(u => u.Driver)
            .WithMany(d => d.DriverApprovalRequests)
            .HasForeignKey(app => app.DriverId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}