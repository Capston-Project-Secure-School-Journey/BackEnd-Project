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

        builder
            .Property(j => j.Id)
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(j => j.SchoolType)
            .HasColumnName("school_type")
            .HasColumnType("tinyint")
            .IsRequired();

        builder
            .Property(j => j.SchoolName)
            .HasColumnName("school_name")
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder
            .Property<string?>(j => j.SchoolDescription)
            .HasColumnName("school_description")
            .HasColumnType("nvarchar(2000)")
            .IsRequired(false);

        builder
            .Property<string>(j => j.Address)
            .HasColumnName("address")
            .HasColumnType("nvarchar(1000)")
            .IsRequired();

        builder
            .Property(j => j.MorningStartTime)
            .HasColumnName("morning_start_time")
            .HasColumnType("time")
            .IsRequired();

        builder
            .Property(j => j.MorningEndTime)
            .HasColumnName("morning_end_time")
            .HasColumnType("time")
            .IsRequired();

        builder
            .Property(j => j.AfternoonStartTime)
            .HasColumnName("afternoon_start_time")
            .HasColumnType("time")
            .IsRequired();
        builder
            .Property(j => j.AfternoonEndTime)
            .HasColumnName("afternoon_end_time")
            .HasColumnType("time")
            .IsRequired();

        builder
            .Property(j => j.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("varchar(11)")
            .IsRequired();

        builder
            .Property(j => j.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(u => u.Images)
            .HasColumnName("images")
            .IsRequired(false)
            .HasColumnType("json");

        builder
            .HasMany(e => e.SchoolPersons)
            .WithOne(p => p.School)
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}