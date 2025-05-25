using System.Linq.Expressions;
using Api.Domain.Models;
using Api.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Api.Domain;

public class Context(DbContextOptions options, IMongoDatabase mongoDatabase) : DbContext(options)
{
    private const string MongoCollectionShuttleScheduleName = "shuttle_schedules";
    private const string MongoCollectionJourneyNoteName = "journey_notes";
    public bool BypassSoftDelete { get; set; } = false;
    public DbSet<User> Users { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<SchoolPerson> SchoolPersons { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<FileManagement> FileManagements { get; set; }
    public DbSet<ClassSchedule> ClassSchedules { get; set; }
    public DbSet<ScheduleGroup> ScheduleGroups { get; set; }
    public DbSet<UserRequestedLog> UserRequestedLogs { get; set; }
    public DbSet<UserBan> UserBans { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SystemVariable> SystemVariables { get; set; }
    public DbSet<DriverApprovalRequest> DriverApprovalRequests { get; set; } = null!;
    public DbSet<DriverRequestStatusHistory> DriverRequestStatusHistories { get; set; } = null!;
    public DbSet<ActiveDriver> ActiveDrivers { get; set; } = null!;
    public IMongoDatabase MongoDatabase => mongoDatabase;

    public IMongoCollection<ShuttleSchedule> ShuttleScheduleCollection =>
        MongoDatabase.GetCollection<ShuttleSchedule>(MongoCollectionShuttleScheduleName);
    public IMongoCollection<JourneyNote> JourneyNoteCollection =>
        MongoDatabase.GetCollection<JourneyNote>(MongoCollectionJourneyNoteName);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Context).Assembly);
        // Apply filter for all entities that inherit from BaseEntity
        foreach (var type in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(BaseModel).IsAssignableFrom(e.ClrType) && e.BaseType == null)
                     .Select(x => x.ClrType))
        {
            var parameter = Expression.Parameter(type, "e");
            var isDeletedProp = Expression.Property(parameter, nameof(BaseModel.IsDeleted));
            var filter = Expression.Lambda(
                Expression.Not(isDeletedProp),
                parameter
            );

            modelBuilder.Entity(type).HasQueryFilter(filter);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        if (!BypassSoftDelete)
            UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        AddTimestamps();
        if (!BypassSoftDelete)
            UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x is { Entity: BaseModel, State: EntityState.Added or EntityState.Modified });
        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((BaseModel)entity.Entity).CreatedAt = DateTimeHelper.GetDateTimeUtc7();
                continue;
            }

            ((BaseModel)entity.Entity).UpdatedAt = DateTimeHelper.GetDateTimeUtc7();
        }
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.Entity is BaseModel))
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.CurrentValues["IsDeleted"] = false;
                    break;
                case EntityState.Deleted:
                    Attach(entry.Entity);
                    ((entry.Entity as BaseModel)!).IsDeleted = true;
                    entry.Property(nameof(BaseModel.IsDeleted)).IsModified = true;
                    break;
            }
    }
}