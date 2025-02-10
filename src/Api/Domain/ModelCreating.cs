using Api.Common.Enums;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain
{
    public static class ModelCreating
    {
        public static ModelBuilder OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().ToTable("users");
            builder.Entity<User>()
                .HasDiscriminator<string>("user_type")
                .HasValue<Driver>("driver")
                .HasValue<Parrent>("parrent")
                .HasValue<User>("user")
                .HasValue<SchoolPerson>("school_person");
            builder.Entity<School>(entity =>
                {
                    entity.HasKey(e => e.Id);
                }
            );


            builder.Entity<School>().ToTable("schools");
            builder.Entity<School>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity
                        .Property<Guid>(j => j.Id)
                        .HasColumnName("id")
                        .HasColumnType("char(36)")
                        .IsRequired();
                    entity
                        .Property<SchoolType>(j => j.SchoolType)
                        .HasColumnName("school_type")
                        .HasColumnType("tinyint")
                        .IsRequired();
                    entity
                        .Property<String?>(j => j.SchoolDescription)
                        .HasColumnName("school_description")
                        .HasColumnType("nvarchar(2000)")
                        .IsRequired(false);
                    entity
                        .Property<string>(j => j.Address)
                        .HasColumnName("address")
                        .HasColumnType("nvarchar(1000)")
                        .IsRequired();
                    entity
                        .Property<TimeSpan>(j => j.MorningStartTime)
                        .HasColumnName("morning_start_time")
                        .HasColumnType("time")
                        .IsRequired();
                    entity
                        .Property<TimeSpan>(j => j.MorningEndTime)
                        .HasColumnName("morning_end_time")
                        .HasColumnType("time")
                        .IsRequired();
                    entity
                        .Property<TimeSpan>(j => j.AfternoonStartTime)
                        .HasColumnName("afternoon_start_time")
                        .HasColumnType("time")
                        .IsRequired();
                    entity
                        .Property<TimeSpan>(j => j.AfternoonEndTime)
                        .HasColumnName("afternoon_end_time")
                        .HasColumnType("time")
                        .IsRequired();
                    entity
                        .Property<string>(j => j.PhoneNumber)
                        .HasColumnName("phone_number")
                        .HasColumnType("varchar(11)")
                        .IsRequired();
                    entity
                        .Property<string?>(j => j.Email)
                        .HasColumnName("email")
                        .HasColumnType("varchar(100)")
                        .IsRequired(false);
                    entity
                        .Property<string?>(j => j.Images)
                        .HasColumnName("images")
                        .HasColumnType("varchar(100)")
                        .IsRequired(false);
                    
                    entity
                        .HasMany(e => e.SchoolPersons)
                        .WithOne(p => p.School)
                        .HasForeignKey(e => e.SchoolId)
                        .OnDelete(DeleteBehavior.NoAction);

                }
            );
            
            builder.Entity<User>().HasQueryFilter(x => x.IsDeleted == false);
            return builder;
        }
    }
}