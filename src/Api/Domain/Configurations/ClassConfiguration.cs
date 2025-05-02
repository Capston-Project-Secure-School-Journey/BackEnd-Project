using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("classes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.SchoolId)
            .IsRequired();

        builder.Property(c => c.Grade)
            .HasColumnName("grade")
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(c => c.ClassName)
            .HasColumnName("class_name")
            .IsRequired()
            .HasColumnType("nvarchar(200)");

        builder.Property(c => c.NumberOfStudent)
            .HasColumnName("number_of_student")
            .IsRequired();

        builder.Property(u => u.ManagedTeachers)
            .HasColumnName("managed_teachers")
            .IsRequired(false)
            .HasColumnType("json");

        builder.HasOne(c => c.School)
            .WithMany()
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}