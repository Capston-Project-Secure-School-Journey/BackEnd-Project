using System.Collections.Concurrent;
using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.Extensions;
using Api.Services.ShuttleScheduleManagementService;
using MongoDB.Driver;

namespace Api.Services.ScanDeviceSchoolTripService;

public class ScanDeviceSchoolTripService(
    Context context,
    IShuttleScheduleManagementService shuttleScheduleManagementService
) : IScanDeviceSchoolTripService
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> JourneyLocks = new();

    public async Task<(ShuttleSchedule, StudentOnBus)> PickUpStudent(Student student)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        var shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);
        var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == student.Id);
        if (studentOnBus.IsPickedUp)
            throw new BadRequestException("Học sinh đã lên xe.");

        var journeyLock = JourneyLocks.GetOrAdd(shuttleSchedule.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();

        try
        {
            // reload NumberOfPickedUpStudents
            shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);

            studentOnBus.IsPickedUp = true;
            studentOnBus.PickedUpTime = currentTime;
            shuttleSchedule.NumberOfPickedUpStudents += 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            journeyLock.Release();
        }

        return (shuttleSchedule, studentOnBus);
    }

    public async Task<(ShuttleSchedule, StudentOnBus)> DropOffStudent(Student student)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        var shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);
        var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == student.Id);

        if (studentOnBus.IsDroppedOff)
            throw new BadRequestException("Học sinh đã xuống xe.");


        var journeyLock = JourneyLocks.GetOrAdd(shuttleSchedule.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();

        try
        {
            // reload NumberOfDroppedOffStudents
            shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);

            studentOnBus.IsDroppedOff = true;
            studentOnBus.DroppedOffTime = currentTime;
            shuttleSchedule.NumberOfDroppedOffStudents += 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            journeyLock.Release();
        }

        return (shuttleSchedule, studentOnBus);
    }

    public async Task<ShuttleSchedule> GetCurrentShuttleScheduleByStudent(Student student)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        var shuttleSchedule = await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == DateOnly.FromDateTime(currentTime)
                        && ss.SchoolId == student.SchoolId
                        && ss.JourneyStatus == JourneyStatus.InProgress
                        && ss.Students.Any(st => st.StudentId == student.Id && !st.SkipPickup))
            .FirstOrDefaultAsync();

        if (shuttleSchedule == null)
        {
            throw new BadRequestException("Bạn không có chuyến đi nào hiện tại.");
        }

        return shuttleSchedule;
    }
}