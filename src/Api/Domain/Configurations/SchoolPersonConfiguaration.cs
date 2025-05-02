using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class SchoolPersonConfiguration : IEntityTypeConfiguration<SchoolPerson>
{
    public void Configure(EntityTypeBuilder<SchoolPerson> builder)
    {
        builder.Property(u => u.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder
            .HasOne(e => e.School)
            .WithMany(p => p.SchoolPersons)
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}