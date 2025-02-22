using Api.Common.Enums;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain
{
    public static class ModelCreating
    {
        public static ModelBuilder OnModelCreating(ModelBuilder builder)
        {
            // user
            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                
                entity.HasDiscriminator<string>("user_type")
                    .HasValue<Driver>("driver")
                    .HasValue<Parrent>("parrent")
                    .HasValue<User>("user")
                    .HasValue<SchoolPerson>("school_person");
                
                entity.HasKey(x => x.Id);
                
                entity
                    .Property(j => j.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(u => u.UserName)
                    .HasColumnName("user_name")
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(u => u.Password)
                    .HasColumnName("password")
                    .IsRequired()
                    .HasColumnType("varchar(1000)");

                entity.Property(u => u.UserType)
                    .HasColumnName("user_type")
                    .IsRequired()
                    .HasColumnType("tinyint");

                entity.Property(u => u.UserTypeName)
                    .HasColumnName("user_type_name")
                    .IsRequired()
                    .HasColumnType("varchar(200)");

                entity.Property(u => u.PhoneNumber)
                    .HasColumnName("phone_number")
                    .IsRequired(false)
                    .HasColumnType("varchar(11)");

                entity.Property(u => u.FirstName)
                    .HasColumnName("first_name")
                    .IsRequired(false)
                    .HasColumnType("nvarchar(200)");

                entity.Property(u => u.LastName)
                    .HasColumnName("last_name")
                    .IsRequired(false)
                    .HasColumnType("nvarchar(200)");

                entity.Property(u => u.Gender)
                    .HasColumnName("gender")
                    .IsRequired()
                    .HasColumnType("tinyint");

                entity.Property(u => u.Email)
                    .HasColumnName("email")
                    .IsRequired(false)
                    .HasColumnType("varchar(100)");

                entity.Property(u => u.DateOfBirth)
                    .HasColumnName("date_of_birth")
                    .IsRequired(false)
                    .HasColumnType("date");

                entity.Property(u => u.Address)
                    .HasColumnName("address")
                    .IsRequired(false)
                    .HasColumnType("nvarchar(1000)");

                entity.Property(u => u.DetailAddress)
                    .HasColumnName("detail_address")
                    .IsRequired(false)
                    .HasColumnType("nvarchar(1000)");

                entity.Property(u => u.AvatarUrl)
                    .HasColumnName("avatar_url")
                    .IsRequired(false)
                    .HasColumnType("varchar(1000)");

                entity.Property(u => u.AccountStatus)
                    .HasColumnName("account_status")
                    .IsRequired()
                    .HasColumnType("tinyint");
            });

            builder.Entity<SchoolPerson>(entity =>
            {
                entity.Property(u => u.SchoolId)
                    .HasColumnName("school_id")
                    .IsRequired();
                
                entity
                    .HasOne(e => e.School)
                    .WithMany(p => p.SchoolPersons)
                    .HasForeignKey(e => e.SchoolId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            builder.Entity<Driver>(entity =>
            {
                entity.Property(u => u.VehicleType)
                    .HasColumnName("vehicle_type")
                    .IsRequired(false)
                    .HasColumnType("tinyint");
                
                entity.Property(u => u.LicenseNumber)
                    .HasColumnName("license_number")
                    .IsRequired(false)
                    .HasColumnType("varchar(15)");
                
                entity.Property(u => u.VerifiedBy)
                    .HasColumnName("verified_by")
                    .IsRequired(false)
                    .HasColumnType("json");
                
                entity.Property(u => u.DriverInformationImage)
                    .HasColumnName("driver_information_image")
                    .IsRequired(false)
                    .HasColumnType("json");

                entity.Property(u => u.LastCheckDrivingLicense)
                    .HasColumnName("last_check_driving_license")
                    .IsRequired(false)
                    .HasColumnType("datetime");
                
                
            });
            
            builder.Entity<Parrent>(entity =>
            {
                entity.Property(u => u.RelationshipWithStudent)
                    .HasColumnName("relationship_with_student")
                    .IsRequired()
                    .HasColumnType("tinyint");
            });
            
            // school
            builder.Entity<School>(entity =>
                {
                    entity.ToTable("schools");
                    
                    entity.HasKey(e => e.Id);
                    
                    entity
                        .Property<Guid>(j => j.Id)
                        .HasColumnName("id")
                        .IsRequired();
                    
                    entity
                        .Property<SchoolType>(j => j.SchoolType)
                        .HasColumnName("school_type")
                        .HasColumnType("tinyint")
                        .IsRequired();
                    
                    entity
                        .Property<string>(j => j.SchoolName)
                        .HasColumnName("school_name")
                        .HasColumnType("nvarchar(100)")
                        .IsRequired();
                    
                    entity
                        .Property<string?>(j => j.SchoolDescription)
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
                    
                    entity.Property(u => u.Images)
                        .HasColumnName("images")
                        .IsRequired(false)
                        .HasColumnType("json");
                    
                    entity
                        .HasMany(e => e.SchoolPersons)
                        .WithOne(p => p.School)
                        .HasForeignKey(e => e.SchoolId)
                        .OnDelete(DeleteBehavior.NoAction);
                }
            );
            
            // class
            builder.Entity<Class>(entity =>
            {
                entity.ToTable("classes");
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Id)
                    .HasColumnName("id")
                    .IsRequired();
                
                entity.Property(c => c.SchoolId)
                    .IsRequired();
        
                entity.Property(c => c.Grade)
                    .HasColumnName("grade")
                    .IsRequired()
                    .HasColumnType("tinyint");
        
                entity.Property(c => c.ClassName)
                    .HasColumnName("class_name")
                    .IsRequired()
                    .HasColumnType("nvarchar(200)");
        
                entity.Property(c => c.NumberOfStudent)
                    .HasColumnName("number_of_student")
                    .IsRequired();
                
                entity.Property(u => u.ManagerTeacher)
                    .HasColumnName("manager_teacher")
                    .IsRequired(false)
                    .HasColumnType("json");
                
                entity.HasOne(c => c.School)
                    .WithMany()
                    .HasForeignKey(c => c.SchoolId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            
            
            // student
            builder.Entity<Student>(entity =>
                {
                    entity.ToTable("students");
                    
                    entity.HasKey(e => e.Id);
                    
                    entity.Property(c => c.Id)
                        .HasColumnName("id")
                        .IsRequired();
                    
                    entity.Property(s => s.SchoolId)
                        .HasColumnName("school_id")
                        .IsRequired();

                    entity.Property(s => s.FirstName)
                        .HasColumnName("first_name")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)");

                    entity.Property(s => s.LastName)
                        .HasColumnName("last_name")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)");

                    entity.Property(s => s.ClassId)
                        .HasColumnName("class_id")
                        .IsRequired();

                    entity.Property(s => s.Gender)
                        .HasColumnName("gender")
                        .IsRequired()
                        .HasColumnType("tinyint");

                    entity.Property(s => s.AvatarKey)
                        .HasColumnName("avatar_key")
                        .IsRequired(false);

                    entity.Property(s => s.QrImageKey)
                        .HasColumnName("qr_image_key")
                        .IsRequired(false);

                    entity.Property(s => s.PickUpLocation)
                        .HasColumnName("pick_up_location")
                        .IsRequired()
                        .HasColumnType("nvarchar(1000)");

                    entity.Property(s => s.PickUpLat)
                        .HasColumnName("pick_up_lat")
                        .HasColumnType("decimal(10,6)")
                        .IsRequired();

                    entity.Property(s => s.PickUpLng)
                        .HasColumnName("pick_up_lng")
                        .HasColumnType("decimal(10,6)")
                        .IsRequired();

                    entity.Property(s => s.LocationGroup)
                        .HasColumnName("location_group")
                        .IsRequired(false);
                    
                    entity.Property(u => u.ManagedBy)
                        .HasColumnName("managed_by")
                        .IsRequired(false)
                        .HasColumnType("json");
                    
                    entity.HasOne(s => s.School)
                        .WithMany()
                        .HasForeignKey(s => s.SchoolId)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(s => s.Class)
                        .WithMany()
                        .HasForeignKey(s => s.ClassId)
                        .OnDelete(DeleteBehavior.NoAction);
                }
            );
            
            
            //teacher
            builder.Entity<Teacher>(entity =>
            {
                entity.ToTable("teachers");

                entity.HasKey(t => t.Id);
                
                entity.Property(c => c.Id)
                    .HasColumnName("id")
                    .IsRequired();
                
                entity.Property(t => t.SchoolId)
                    .HasColumnName("school_id")
                    .IsRequired();

                entity.Property(t => t.FirstName)
                    .HasColumnName("first_name")
                    .IsRequired()
                    .HasColumnType("nvarchar(200)");

                entity.Property(t => t.LastName)
                    .HasColumnName("last_name")
                    .IsRequired()
                    .HasColumnType("nvarchar(200)");

                entity.Property(t => t.DateOfBirth)
                    .HasColumnName("date_of_birth")
                    .IsRequired()
                    .HasColumnType("date");

                entity.Property(t => t.Gender)
                    .HasColumnName("gender")
                    .IsRequired()
                    .HasColumnType("tinyint");

                entity.Property(t => t.PhoneNumber)
                    .HasColumnName("phone_number")
                    .IsRequired()
                    .HasColumnType("varchar(11)");

                entity.Property(t => t.Email)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(t => t.AvatarKey)
                    .HasColumnName("avatar_key")
                    .IsRequired(false);

                entity.HasOne(t => t.School)
                    .WithMany()
                    .HasForeignKey(t => t.SchoolId)
                    .OnDelete(DeleteBehavior.NoAction);

            });
            
            builder.Entity<User>().HasQueryFilter(x => x.IsDeleted == false);
            builder.Entity<School>().HasQueryFilter(x => x.IsDeleted == false);
            builder.Entity<Student>().HasQueryFilter(x => x.IsDeleted == false);
            builder.Entity<Teacher>().HasQueryFilter(x => x.IsDeleted == false);
            builder.Entity<Class>().HasQueryFilter(x => x.IsDeleted == false);
            
            return builder;
        }
    }
}