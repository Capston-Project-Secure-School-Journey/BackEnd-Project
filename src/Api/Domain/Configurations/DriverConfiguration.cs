using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.Property(u => u.VehicleType)
            .HasColumnName("vehicle_type")
            .IsRequired(false)
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(u => u.LicenseNumber)
            .HasColumnName("license_number")
            .IsRequired(false)
            .HasColumnType(Constants.Varchar(50));

        builder.Property(u => u.VerifiedBy)
            .HasColumnName("verified_by")
            .IsRequired(false)
            .HasColumnType(Constants.Json);

        builder.Property(u => u.DriverInformationImages)
            .HasColumnName("driver_information_images")
            .IsRequired(false)
            .HasColumnType(Constants.Json);

        builder.Property(u => u.VehicleImages)
            .HasColumnName("vehicle_images")
            .IsRequired(false)
            .HasColumnType(Constants.Json);

        builder.Property(u => u.LastCheckDrivingLicense)
            .HasColumnName("last_check_driving_license")
            .IsRequired(false)
            .HasColumnType(Constants.Datetime);
    }
}