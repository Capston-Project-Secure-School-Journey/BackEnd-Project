using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(e => e.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(s => s.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(s => s.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(s => s.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(p => p.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasColumnType(Constants.Nvarchar(400))
            .HasComputedColumnSql("CONCAT(first_name, ' ', last_name)");

        builder.Property(s => s.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired()
            .HasColumnType(Constants.Date);

        builder.Property(s => s.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(s => s.Gender)
            .HasColumnName("gender")
            .IsRequired()
            .HasColumnType(Constants.Tinyint);

        builder.Property(s => s.AvatarKey)
            .HasColumnName("avatar_key")
            .IsRequired(false);

        builder.Property(s => s.QrImageKey)
            .HasColumnName("qr_image_key")
            .IsRequired(false);

        builder.Property(s => s.PickUpLocation)
            .HasColumnName("pick_up_location")
            .IsRequired()
            .HasColumnType(Constants.Nvarchar(1000));

        builder.Property(s => s.PickUpLat)
            .HasColumnName("pick_up_lat")
            .HasColumnType(Constants.Double)
            .IsRequired();

        builder.Property(s => s.PickUpLng)
            .HasColumnName("pick_up_lng")
            .HasColumnType(Constants.Double)
            .IsRequired();

        builder.Property(s => s.LocationGroup)
            .HasColumnName("location_group")
            .IsRequired(false);

        builder.Property(u => u.ManagedBy)
            .HasColumnName("managed_by")
            .IsRequired(false)
            .HasColumnType(Constants.Json);

        builder.Property(u => u.LastTimeUpdatedPickupLocation)
            .HasColumnName("last_time_updated_pickup_location")
            .IsRequired(false)
            .HasColumnType(Constants.Datetime);

        builder.Property(u => u.NeedsPickup)
            .HasColumnName("needs_pickup")
            .IsRequired()
            .HasColumnType(Constants.Tinyint);

        builder.HasOne(s => s.School)
            .WithMany()
            .HasForeignKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.Class)
            .WithMany(cl => cl.Students)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}