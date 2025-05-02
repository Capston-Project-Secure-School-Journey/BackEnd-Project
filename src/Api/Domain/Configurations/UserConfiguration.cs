using Api.Common.Utilities;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasDiscriminator<string>("discriminator")
            .HasValue<Driver>("driver")
            .HasValue<Parent>("parent")
            .HasValue<User>("user")
            .HasValue<SchoolPerson>("school_person");

        builder.HasKey(x => x.Id);

        builder
            .Property(j => j.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasColumnName("user_name")
            .IsRequired()
            .HasColumnType(Constants.Varchar(100));

        builder.Property(u => u.Password)
            .HasColumnName("password")
            .IsRequired()
            .HasColumnType(Constants.Varchar(1000));

        builder.Property(u => u.UserType)
            .HasColumnName("user_type")
            .IsRequired()
            .HasColumnType(Constants.Tinyint);

        builder.Property(u => u.UserTypeName)
            .HasColumnName("user_type_name")
            .IsRequired()
            .HasColumnType(Constants.Varchar(200));

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired(false)
            .HasColumnType(Constants.Varchar(11));

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .IsRequired(false)
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .IsRequired(false)
            .HasColumnType(Constants.Nvarchar(200));

        builder.Property(u => u.Gender)
            .HasColumnName("gender")
            .IsRequired()
            .HasColumnType(Constants.Tinyint);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired(false)
            .HasColumnType(Constants.Varchar(100));

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired(false)
            .HasColumnType(Constants.Date);

        builder.Property(u => u.Address)
            .HasColumnName("address")
            .IsRequired(false)
            .HasColumnType(Constants.Nvarchar(1000));

        builder.Property(u => u.DetailAddress)
            .HasColumnName("detail_address")
            .IsRequired(false)
            .HasColumnType(Constants.Nvarchar(1000));

        builder.Property(u => u.AvatarKey)
            .HasColumnName("avatar_key")
            .IsRequired(false);

        builder.Property(u => u.AccountStatus)
            .HasColumnName("account_status")
            .IsRequired()
            .HasColumnType(Constants.Tinyint);

        builder.Property(u => u.VerificationMethod)
            .HasColumnName("verification_method")
            .IsRequired(false)
            .HasColumnType(Constants.Tinyint);

        builder.Property(u => u.DeviceTokens)
            .HasColumnName("device_tokens")
            .HasColumnType(Constants.Json);
    }
}