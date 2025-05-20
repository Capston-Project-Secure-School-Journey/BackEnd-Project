using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.Extensions;
using Api.Services.ChildrenManagementService;
using Api.Services.ShuttleScheduleManagementService;
using MongoDB.Driver;

namespace Api.Services.ScanDeviceSchoolTripService;

public class ScanDeviceSchoolTripService(
    Context context,
    IShuttleScheduleManagementService shuttleScheduleManagementService,
    IChildrenManagementService childrenManagementService): IScanDeviceSchoolTripService
{
    private static readonly SemaphoreSlim DropOffLock = new(1, 1);
    private static readonly SemaphoreSlim PickUpLock = new(1, 1);
    
    public async Task PickUpStudent(string studentHash)
    {
        await PickUpLock.WaitAsync();

        try
        {
            var currentTime = DateTimeHelper.GetDateTimeUtc7();
            var student = await childrenManagementService.FindStudentWithHash(studentHash);
            var shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);
            var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == student.Id);

            if (studentOnBus.IsPickedUp)
                throw new BadRequestException("Học sinh đã lên xe.");
            
            studentOnBus.IsPickedUp = true;
            studentOnBus.PickedUpTime = currentTime;
            shuttleSchedule.NumberOfPickedUpStudents += 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            PickUpLock.Release();
        }
    }

    public async Task DropOffStudent(string studentHash)
    {
        await DropOffLock.WaitAsync();

        try
        {
            var currentTime = DateTimeHelper.GetDateTimeUtc7();
            var student = await childrenManagementService.FindStudentWithHash(studentHash);
            var shuttleSchedule = await GetCurrentShuttleScheduleByStudent(student);
            var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == student.Id);

            if (studentOnBus.IsDroppedOff)
                throw new BadRequestException("Học sinh đã xuống xe.");
            
            studentOnBus.IsDroppedOff = true;
            studentOnBus.DroppedOffTime = currentTime;
            shuttleSchedule.NumberOfDroppedOffStudents += 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            DropOffLock.Release();
        }
    }
    
    private async Task<ShuttleSchedule> GetCurrentShuttleScheduleByStudent(Student student)
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