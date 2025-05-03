using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.ToTable("class_schedules");

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(t => t.SessionType)
            .HasColumnName("session_type")
            .HasColumnType(Constants.Tinyint)
            .IsRequired();

        builder.Property(t => t.Date)
            .HasColumnName("date")
            .HasColumnType(Constants.Date)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasColumnName("note")
            .HasColumnType(Constants.Nvarchar(1000))
            .IsRequired(false);

        builder.Property(t => t.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(t => t.Grade)
            .HasColumnName("grade")
            .HasColumnType(Constants.Tinyint)
            .IsRequired(false);

        builder.Property(t => t.ScheduleType)
            .HasColumnName("schedule_type")
            .HasColumnType(Constants.Tinyint)
            .IsRequired();

        builder.HasOne(t => t.School)
            .WithMany(sc => sc.ClassSchedules)
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Class)
            .WithMany()
            .HasForeignKey(t => t.ClassId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(t => t.ScheduleGroupId)
            .HasColumnName("schedule_group_id")
            .IsRequired(false);

        builder.HasOne(t => t.ScheduleGroup)
            .WithMany()
            .HasForeignKey(t => t.ScheduleGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}