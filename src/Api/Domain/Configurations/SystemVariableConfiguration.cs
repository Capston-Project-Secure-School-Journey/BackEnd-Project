using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class SystemVariableConfiguration : IEntityTypeConfiguration<SystemVariable>
{
    public void Configure(EntityTypeBuilder<SystemVariable> builder)
    {
        builder.ToTable("system_variables");

        builder.HasKey(e => new { e.SchoolId, e.Name });

        builder.Property(u => u.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasColumnType(Constants.Varchar(100))
            .IsRequired();

        builder.Property(u => u.Value)
            .HasColumnName("value")
            .HasColumnType(Constants.Nvarchar(1000))
            .IsRequired();
    }
}