using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ShuttleScheduleService;
using Api.DTOs.Scheduling;
using Api.Extensions;
using Api.Scheduling;
using Api.Services.ShuttleScheduleManagementService;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class CreateShuttleScheduleJob(
    IServiceProvider serviceProvider,
    ILogger<CreateShuttleScheduleJob> logger,
    IStudentGroupingAlgorithm groupingAlgorithm,
    IShuttleScheduleManagementService shuttleScheduleManagementService) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        var schoolId = Guid.Parse(args[0].ToString()!);

        if (schoolId == Guid.Empty)
            throw new InvalidDataException(ErrorMessages.InvalidSchoolId);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();

            var startDate = await db.SystemVariables
                .FirstOrDefaultAsync(e => e.Name == "START_DATE" && e.SchoolId == schoolId);

            if (startDate is null)
                return;
            await HasStudentMissingAddress(db, schoolId);
            await IsDriverCapacityInsufficient(db, schoolId);

            var nextWeekRange = DateTimeHelper.GetNextWeekRange(DateTimeHelper.GetDateTimeOnlyUtc7());
            var schedules = await db.ClassSchedules
                .AsNoTracking()
                .AsQueryable()
                .Where(x => x.SchoolId == schoolId)
                .Where(x => x.Date >= nextWeekRange.StartOfWeek && x.Date <= nextWeekRange.EndOfWeek)
                .ToListAsync();

            var drivers = await db.DriverApprovalRequests
                .AsNoTracking()
                .AsQueryable()
                .Where(d => d.SchoolId == schoolId && d.RequestStatus == RequestStatus.Approved)
                .Select(x => new DriverData()
                {
                    Id = x.DriverId,
                    SeatingCapacity = x.SeatingCapacity,
                    Used = 0
                })
                .ToListAsync();

            var studentsPerSession = await GroupStudentsBySessionAsync(schedules, db, schoolId);

            var requests = new List<CreateShuttleScheduleDto>();
            foreach (var key in studentsPerSession.Keys)
            {
                var groups = groupingAlgorithm.AllocateStudentsToBuses(studentsPerSession[key], ref drivers);

                foreach (var group in groups)
                {
                    requests.Add(new CreateShuttleScheduleDto()
                    {
                        DriverId = group.Key.Id,
                        Date = key.Item1,
                        SchoolId = schoolId,
                        SessionType = key.Item2,
                        Students = group.Value,
                        Type = ShuttleScheduleType.PickUp,
                    });
                    requests.Add(new CreateShuttleScheduleDto()
                    {
                        DriverId = group.Key.Id,
                        Date = key.Item1,
                        SchoolId = schoolId,
                        SessionType = key.Item2,
                        Students = group.Value,
                        Type = ShuttleScheduleType.DropOff,
                    });
                }
            }
            await shuttleScheduleManagementService.AddShuttleSchedule(requests);
            
            logger.LogInformation("CreateShuttleScheduleJob successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "CreateShuttleScheduleJob failed");
        }
    }

    private async Task HasStudentMissingAddress(Context db, Guid schoolId)
    {
        var studentMissingAddress = await db.Students
            .AsNoTracking()
            .AsQueryable()
            .Include(s => s.Class)
            .Where(s => s.SchoolId == schoolId)
            .Where(s => s.NeedsPickup)
            .Where(s => string.IsNullOrEmpty(s.PickUpLocation))
            .CountAsync();

        if (studentMissingAddress > 0)
        {
            logger.LogError("Has student missing address");
            throw new InvalidDataException("Has student missing address");
        }
    }

    private async Task IsDriverCapacityInsufficient(Context db, Guid schoolId)
    {
        var studentCount = await db.Students
            .Where(s => s.SchoolId == schoolId && s.NeedsPickup)
            .Where(s => s.NeedsPickup)
            .CountAsync();

        var seatingCapacityCount = await db.DriverApprovalRequests
            .AsNoTracking()
            .AsQueryable()
            .Where(d => d.SchoolId == schoolId && d.RequestStatus == RequestStatus.Approved)
            .SumAsync(d => d.SeatingCapacity);

        if (studentCount > seatingCapacityCount)
        {
            logger.LogError("Is driver missing capacity");
            throw new InvalidDataException("Is driver missing capacity");
        }
    }

    private static async Task<Dictionary<(DateOnly, SessionType), List<Student>>> GroupStudentsBySessionAsync(
        List<ClassSchedule> schedules, Context db, Guid schoolId)
    {
        var studentsPerSession = new Dictionary<(DateOnly, SessionType), List<Student>>();
        var studentCache = new Dictionary<Guid, List<Student>>();

        foreach (var schedule in schedules)
        {
            var students = await GetStudentsAsync(studentCache, db, schoolId, schedule.ClassId);

            foreach (var sessionType in ExpandSessionTypes(schedule.SessionType))
            {
                var key = (schedule.Date, sessionType);
                if (studentsPerSession.TryGetValue(key, out var existing))
                    studentsPerSession[key] = existing.Union(students).ToList();
                else
                    studentsPerSession[key] = [..students];
            }
        }

        return studentsPerSession;
    }

    private static IEnumerable<SessionType> ExpandSessionTypes(SessionType type)
    {
        return type == SessionType.FullDay
            ? new[] { SessionType.Morning, SessionType.Afternoon }
            : new[] { type };
    }

    private static async Task<List<Student>> GetStudentsAsync(
        Dictionary<Guid, List<Student>> cache, Context db, Guid schoolId, Guid classId)
    {
        if (!cache.TryGetValue(classId, out var students))
        {
            students = await db.Students
                .AsNoTracking()
                .Include(st => st.Class)
                .Where(st => st.SchoolId == schoolId && st.ClassId == classId && st.NeedsPickup)
                .ToListAsync();
            cache[classId] = students;
        }

        return students;
    }
}