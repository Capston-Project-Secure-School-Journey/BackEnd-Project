using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain;

public class Context : DbContext
{
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
    public DbSet<DriverApprovalRequest> DriverApprovalRequests { get; set; }
    public DbSet<DriverRequestStatusHistory> DriverRequestStatusHistories { get; set; }
    
    public Context(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelCreating.OnModelCreating(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        AddTimestamps();
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));
        foreach (var entity in entities)
        {
            var now = DateTimeOffset.Now;
            if (entity.State == EntityState.Added)
            {
                ((BaseModel)entity.Entity).CreatedAt = now;
                continue;
            }

            ((BaseModel)entity.Entity).UpdatedAt = now;
        }
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries())
            if (entry.Entity is BaseModel)
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.CurrentValues["IsDeleted"] = false;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.CurrentValues["IsDeleted"] = true;
                        break;
                }
    }
}