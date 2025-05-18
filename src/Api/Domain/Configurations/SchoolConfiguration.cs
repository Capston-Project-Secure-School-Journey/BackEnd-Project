using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");

        builder.HasKey(e => e.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(j => j.SchoolType)
            .HasColumnName("school_type")
            .HasColumnType(Constants.Tinyint)
            .IsRequired();

        builder.Property(j => j.SchoolName)
            .HasColumnName("school_name")
            .HasColumnType(Constants.Nvarchar(100))
            .IsRequired();

        builder.Property<string?>(j => j.SchoolDescription)
            .HasColumnName("school_description")
            .HasColumnType(Constants.Nvarchar(2000))
            .IsRequired(false);

        builder.Property<string>(j => j.Address)
            .HasColumnName("address")
            .HasColumnType(Constants.Nvarchar(1000))
            .IsRequired();
        
        builder.Property(j => j.AddressLat)
            .HasColumnName("address_lat")
            .HasColumnType(Constants.Double)
            .IsRequired();
        
        builder.Property(j => j.AddressLng)
            .HasColumnName("address_lng")
            .HasColumnType(Constants.Double)
            .IsRequired();

        builder.Property(j => j.MorningStartTime)
            .HasColumnName("morning_start_time")
            .HasColumnType(Constants.Time)
            .IsRequired();

        builder.Property(j => j.MorningEndTime)
            .HasColumnName("morning_end_time")
            .HasColumnType(Constants.Time)
            .IsRequired();

        builder.Property(j => j.AfternoonStartTime)
            .HasColumnName("afternoon_start_time")
            .HasColumnType(Constants.Time)
            .IsRequired();

        builder.Property(j => j.AfternoonEndTime)
            .HasColumnName("afternoon_end_time")
            .HasColumnType(Constants.Time)
            .IsRequired();

        builder.Property(j => j.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType(Constants.Varchar(11))
            .IsRequired();

        builder.Property(j => j.Email)
            .HasColumnName("email")
            .HasColumnType(Constants.Varchar(100))
            .IsRequired(false);

        builder.Property(u => u.Images)
            .HasColumnName("images")
            .IsRequired(false)
            .HasColumnType(Constants.Json);

        builder.HasMany(e => e.SchoolPersons)
            .WithOne(p => p.School)
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}