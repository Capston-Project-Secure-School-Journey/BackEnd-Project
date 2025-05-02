using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(t => t.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasColumnType("nvarchar(200)");

        builder.Property(t => t.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasColumnType("nvarchar(200)");

        builder
            .Property(p => p.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasColumnType("nvarchar(400)")
            .HasComputedColumnSql("CONCAT(first_name, ' ', last_name)");

        builder.Property(t => t.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(t => t.Gender)
            .HasColumnName("gender")
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(t => t.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired()
            .HasColumnType("varchar(11)");

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasColumnType("varchar(100)");

        builder.Property(t => t.AvatarKey)
            .HasColumnName("avatar_key")
            .IsRequired(false);

        builder.HasOne(t => t.School)
            .WithMany()
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}