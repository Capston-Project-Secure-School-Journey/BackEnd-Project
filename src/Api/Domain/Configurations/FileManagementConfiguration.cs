using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class FileManagementConfiguration : IEntityTypeConfiguration<FileManagement>
{
    public void Configure(EntityTypeBuilder<FileManagement> builder)
    {
        builder.ToTable("file_managements");

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.FileName)
            .HasColumnName("file_name")
            .HasColumnType("nvarchar(200)")
            .IsRequired();

        builder.Property(t => t.S3Key)
            .HasColumnName("s3_key")
            .IsRequired()
            .HasColumnType("varchar(2000)");

        builder.Property(t => t.FileType)
            .HasColumnName("file_type")
            .IsRequired()
            .HasColumnType("varchar(100)");

        builder.Property(t => t.FileSize)
            .HasColumnName("file_size")
            .IsRequired()
            .HasColumnType("float");

        builder.Property(t => t.UploadDate)
            .HasColumnName("upload_date")
            .IsRequired()
            .HasColumnType("timestamp");

        builder.Property(t => t.UploadBy)
            .HasColumnName("uploaded_by")
            .IsRequired(false);

        builder.Property(t => t.RelatedObjectId)
            .HasColumnName("related_object_id")
            .IsRequired(false);

        builder.Property(t => t.RelatedObjectType)
            .HasColumnName("related_object_type")
            .IsRequired(false)
            .HasColumnType("tinyint");

        builder.Property(t => t.IsUploaded)
            .HasColumnName("is_uploaded")
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnType("bit");
    }
}