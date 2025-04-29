using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain;

public static class ModelCreating
{
    public static ModelBuilder OnModelCreating(ModelBuilder builder)
    {
        // user
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasDiscriminator<string>("discriminator")
                .HasValue<Driver>("driver")
                .HasValue<Parent>("parent")
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

            entity.Property(u => u.AvatarKey)
                .HasColumnName("avatar_url")
                .IsRequired(false)
                .HasColumnType("char(36)");

            entity.Property(u => u.AccountStatus)
                .HasColumnName("account_status")
                .IsRequired()
                .HasColumnType("tinyint");

            entity.Property(u => u.VerificationMethod)
                .HasColumnName("verification_method")
                .IsRequired(false)
                .HasColumnType("tinyint");

            entity.Property(u => u.DeviceTokens)
                .HasColumnName("device_tokens")
                .HasColumnType("json");
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
                .HasColumnType("nvarchar(200)");

            entity.Property(u => u.LicenseNumber)
                .HasColumnName("license_number")
                .IsRequired(false)
                .HasColumnType("varchar(50)");

            entity.Property(u => u.VerifiedBy)
                .HasColumnName("verified_by")
                .IsRequired(false)
                .HasColumnType("json");

            entity.Property(u => u.DriverInformationImages)
                .HasColumnName("driver_information_images")
                .IsRequired(false)
                .HasColumnType("json");

            entity.Property(u => u.VehicleImages)
                .HasColumnName("vehicle_images")
                .IsRequired(false)
                .HasColumnType("json");

            entity.Property(u => u.LastCheckDrivingLicense)
                .HasColumnName("last_check_driving_license")
                .IsRequired(false)
                .HasColumnType("datetime");
        });

        builder.Entity<Parent>(entity =>
        {
            entity.Property(u => u.RelationshipWithStudents)
                .HasColumnName("relationship_with_students")
                .IsRequired(false)
                .HasColumnType("json");
        });

        // school
        builder.Entity<School>(entity =>
            {
                entity.ToTable("schools");

                entity.HasKey(e => e.Id);

                entity
                    .Property(j => j.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity
                    .Property(j => j.SchoolType)
                    .HasColumnName("school_type")
                    .HasColumnType("tinyint")
                    .IsRequired();

                entity
                    .Property(j => j.SchoolName)
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
                    .Property(j => j.MorningStartTime)
                    .HasColumnName("morning_start_time")
                    .HasColumnType("time")
                    .IsRequired();

                entity
                    .Property(j => j.MorningEndTime)
                    .HasColumnName("morning_end_time")
                    .HasColumnType("time")
                    .IsRequired();

                entity
                    .Property(j => j.AfternoonStartTime)
                    .HasColumnName("afternoon_start_time")
                    .HasColumnType("time")
                    .IsRequired();
                entity
                    .Property(j => j.AfternoonEndTime)
                    .HasColumnName("afternoon_end_time")
                    .HasColumnType("time")
                    .IsRequired();

                entity
                    .Property(j => j.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasColumnType("varchar(11)")
                    .IsRequired();

                entity
                    .Property(j => j.Email)
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

            entity.Property(u => u.ManagedTeachers)
                .HasColumnName("managed_teachers")
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

                entity
                    .Property(p => p.FullName)
                    .HasColumnName("full_name")
                    .IsRequired()
                    .HasColumnType("nvarchar(400)")
                    .HasComputedColumnSql("CONCAT(first_name, ' ', last_name)");

                entity.Property(s => s.DateOfBirth)
                    .HasColumnName("date_of_birth")
                    .IsRequired()
                    .HasColumnType("date");

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

                entity.Property(u => u.LastTimeUpdatedPickupLocation)
                    .HasColumnName("last_time_updated_pickup_location")
                    .IsRequired(false)
                    .HasColumnType("datetime");

                entity.Property(u => u.NeedsPickup)
                    .HasColumnName("needs_pickup")
                    .IsRequired()
                    .HasColumnType("tinyint");

                entity.HasOne(s => s.School)
                    .WithMany()
                    .HasForeignKey(s => s.SchoolId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.Class)
                    .WithMany(cl => cl.Students)
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

            entity
                .Property(p => p.FullName)
                .HasColumnName("full_name")
                .IsRequired()
                .HasColumnType("nvarchar(400)")
                .HasComputedColumnSql("CONCAT(first_name, ' ', last_name)");

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

        //file management
        builder.Entity<FileManagement>(entity =>
        {
            entity.ToTable("file_managements");

            entity.HasKey(t => t.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(t => t.FileName)
                .HasColumnName("file_name")
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            entity.Property(t => t.S3Key)
                .HasColumnName("s3_key")
                .IsRequired()
                .HasColumnType("varchar(2000)");

            entity.Property(t => t.FileType)
                .HasColumnName("file_type")
                .IsRequired()
                .HasColumnType("varchar(100)");

            entity.Property(t => t.FileSize)
                .HasColumnName("file_size")
                .IsRequired()
                .HasColumnType("float");

            entity.Property(t => t.UploadDate)
                .HasColumnName("upload_date")
                .IsRequired()
                .HasColumnType("timestamp");

            entity.Property(t => t.UploadBy)
                .HasColumnName("uploaded_by")
                .IsRequired(false)
                .HasColumnType("char(36)");

            entity.Property(t => t.RelatedObjectId)
                .HasColumnName("related_object_id")
                .IsRequired(false)
                .HasColumnType("char(36)");

            entity.Property(t => t.RelatedObjectType)
                .HasColumnName("related_object_type")
                .IsRequired(false)
                .HasColumnType("tinyint");

            entity.Property(t => t.IsUploaded)
                .HasColumnName("is_uploaded")
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnType("bit");
        });

        //class schedule
        builder.Entity<ClassSchedule>(entity =>
        {
            entity.ToTable("class_schedules");

            entity.HasKey(t => t.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(t => t.SchoolId)
                .HasColumnName("school_id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(t => t.SessionType)
                .HasColumnName("session_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.Property(t => t.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(t => t.Note)
                .HasColumnName("note")
                .HasColumnType("nvarchar(1000)")
                .IsRequired(false);

            entity.Property(t => t.ClassId)
                .HasColumnName("class_id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(t => t.Grade)
                .HasColumnName("grade")
                .HasColumnType("tinyint")
                .IsRequired(false);

            entity.Property(t => t.ScheduleType)
                .HasColumnName("schedule_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.HasOne(t => t.School)
                .WithMany(sc => sc.ClassSchedules)
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(t => t.Class)
                .WithMany()
                .HasForeignKey(t => t.ClassId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(t => t.ScheduleGroupId)
                .HasColumnName("schedule_group_id")
                .HasColumnType("char(36)")
                .IsRequired(false);

            entity.HasOne(t => t.ScheduleGroup)
                .WithMany()
                .HasForeignKey(t => t.ScheduleGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // schedule group
        builder.Entity<ScheduleGroup>(entity =>
        {
            entity.ToTable("schedule_groups");

            entity.HasKey(t => t.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(t => t.SchoolId)
                .HasColumnName("school_id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(t => t.SessionType)
                .HasColumnName("session_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.Property(t => t.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(t => t.Grade)
                .HasColumnName("grade")
                .HasColumnType("tinyint")
                .IsRequired(false);

            entity.Property(t => t.ScheduleType)
                .HasColumnName("schedule_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.HasOne(t => t.School)
                .WithMany()
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        //user requested log
        builder.Entity<UserRequestedLog>(entity =>
        {
            entity.ToTable("user_requested_logs");

            entity.HasKey(t => t.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .HasColumnType("int unsigned")
                .UseMySqlIdentityColumn()
                .IsRequired();

            entity.Property(c => c.UserId)
                .HasColumnName("user_id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(c => c.UserRequestedType)
                .HasColumnName("user_requested_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.Property(c => c.DatetimeRequested)
                .HasColumnName("datetime_requested")
                .HasColumnType("datetime")
                .IsRequired();
        });

        //user ban
        builder.Entity<UserBan>(entity =>
        {
            entity.ToTable("user_bans");

            entity.HasKey(t => t.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .HasColumnType("int unsigned")
                .UseMySqlIdentityColumn()
                .IsRequired();

            entity.Property(c => c.UserId)
                .HasColumnName("user_id")
                .HasColumnType("char(36)")
                .IsRequired();

            entity.Property(c => c.BanType)
                .HasColumnName("ban_type")
                .HasColumnType("tinyint")
                .IsRequired();

            entity.Property(c => c.BanDate)
                .HasColumnName("ban_date")
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(c => c.BanExpiryDate)
                .HasColumnName("ban_expiry_date")
                .HasColumnType("datetime")
                .IsRequired();
        });

        // Notification
        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnName("content")
                .HasColumnType("nvarchar(1000)")
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasColumnType("tinyint");

            entity.Property(e => e.RecipientId)
                .HasColumnName("recipient_id")
                .IsRequired();

            entity.Property(e => e.SenderId)
                .HasColumnName("sender_id")
                .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("createdAt")
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(e => e.IsRead)
                .HasColumnName("is_read")
                .HasColumnType("bit")
                .IsRequired();

            entity.Property(e => e.Navigation)
                .HasColumnName("navigation")
                .HasColumnType("varchar(300)")
                .IsRequired(false);

            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasColumnType("tinyint");

            entity.HasOne(e => e.Recipient)
                .WithMany()
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // SystemVariable
        builder.Entity<SystemVariable>(entity =>
        {
            entity.ToTable("Notification");

            entity.HasKey(e => new { e.SchoolId, e.Name });

            entity.Property(u => u.SchoolId)
                .HasColumnName("school_id")
                .IsRequired();

            entity.Property(u => u.Name)
                .HasColumnName("name")
                .HasColumnType("varchar(100)")
                .IsRequired();

            entity.Property(u => u.Value)
                .HasColumnName("value")
                .HasColumnType("nvarchar(1000)")
                .IsRequired();
        });
        
        // DriverApprovalRequest
        builder.Entity<DriverApprovalRequest>(entity =>
        {
            entity.ToTable("driver_approval_requests");

            entity.HasKey(e => e.Id );

            entity.Property(u => u.SchoolId)
                .HasColumnName("school_id")
                .IsRequired();
    
            entity.Property(u => u.RequestedDate)
                .HasColumnName("requested_date")
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(u => u.DriverId)
                .HasColumnName("driver_id")
                .IsRequired();
            
            entity.Property(u => u.RequestStatus)
                .HasColumnName("request_status")
                .HasColumnType("tinyint")
                .IsRequired();
            
            entity.Property(u => u.ApprovedBy)
                .HasColumnName("approved_by")
                .IsRequired(false);
            
            entity.Property(u => u.VehicleType)
                .HasColumnName("vehicle_type")
                .IsRequired()
                .HasColumnType("nvarchar(200)");

            entity.Property(u => u.LicenseNumber)
                .HasColumnName("license_number")
                .IsRequired()
                .HasColumnType("varchar(50)");

            entity.Property(u => u.DriverInformationImages)
                .HasColumnName("driver_information_images")
                .IsRequired()
                .HasColumnType("json");

            entity.Property(u => u.VehicleImages)
                .HasColumnName("vehicle_images")
                .IsRequired()
                .HasColumnType("json");

            entity.Property(u => u.LastCheckDrivingLicense)
                .HasColumnName("last_check_driving_license")
                .IsRequired(false)
                .HasColumnType("datetime");
            
            entity.HasMany(e => e.DriverRequestStatusHistories)
                .WithOne(x => x.Request)
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // DriverRequestStatusHistory
        builder.Entity<DriverRequestStatusHistory>(entity =>
        {
            entity.ToTable("driver_request_status_histories");

            entity.HasKey(e => e.Id);

            entity.Property(u => u.RequestId)
                .HasColumnName("request_id")
                .IsRequired();
            
            entity.Property(u => u.FromStatus)
                .HasColumnName("from_status")
                .HasColumnType("tinyint")
                .IsRequired(false);
            
            entity.Property(u => u.ToStatus)
                .HasColumnName("to_status")
                .HasColumnType("tinyint")
                .IsRequired();
            
            entity.Property(u => u.ChangedBy)
                .HasColumnName("changed_by")
                .IsRequired();
            
            entity.Property(u => u.ChangedAt)
                .HasColumnName("changed_at")
                .HasColumnType("datetime")
                .IsRequired();
            
            entity.Property(u => u.Note)
                .HasColumnName("note")
                .HasColumnType("nvarchar(1000)")
                .IsRequired();
        });
        
        builder.Entity<User>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<School>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Student>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Teacher>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<Class>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<FileManagement>().HasQueryFilter(x => x.IsDeleted == false);
        builder.Entity<ClassSchedule>().HasQueryFilter(x => x.IsDeleted == false);

        return builder;
    }
}