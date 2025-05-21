using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class ActiveDriverConfiguration : IEntityTypeConfiguration<ActiveDriver>
{
    public void Configure(EntityTypeBuilder<ActiveDriver> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.DriverId)
            .HasColumnName("driver_id")
            .IsRequired();
            
        builder.Property(x => x.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();
            
        builder.Property(x => x.SeatingCapacity)
            .HasColumnType("tinyint")
            .HasColumnName("seating_capacity")
            .IsRequired();
            
        builder.Property(x => x.Used)
            .HasColumnType("int")
            .HasColumnName("used")
            .HasDefaultValue(0);

        builder.Property(x => x.ExpiredAt)
            .IsRequired(false);

        builder.HasOne(x => x.Driver)
            .WithMany()
            .HasForeignKey(x => x.DriverId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(x => x.School)
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasIndex(x => new { x.DriverId, x.SchoolId });
    }
} 