using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.Property(u => u.RelationshipWithStudents)
            .HasColumnName("relationship_with_students")
            .IsRequired(false)
            .HasColumnType(Constants.Json);
    }
}