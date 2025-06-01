using Api.Common.Enums;
using Api.Domain.Models;
using Api.Extensions;
using Api.Jobs.SchoolTripEventDispatchJobs;
using Api.Services.ChildrenManagementService;
using Hangfire;

namespace Api.Services.ScanDeviceSchoolTripService;

public class ScanDeviceSchoolTripHandler(
    IScanDeviceSchoolTripService scanDeviceSchoolTripService,
    IChildrenManagementService childrenManagementService)
    : IScanDeviceSchoolTripHandler
{
    private async Task PickUpStudent(Student student)
    {
        var (shuttleSchedule, studentOnBus) = await scanDeviceSchoolTripService.PickUpStudent(student);
        BackgroundJob.Enqueue<SendStudentTripEventJob>(
            (job) => job.ExecuteAsync(shuttleSchedule.Id, StudentTripEvent.PickedUp, studentOnBus.StudentId));
    }

    private async Task DropOffStudent(Student student)
    {
        var (shuttleSchedule, studentOnBus) = await scanDeviceSchoolTripService.DropOffStudent(student);
        BackgroundJob.Enqueue<SendStudentTripEventJob>(
            (job) => job.ExecuteAsync(shuttleSchedule.Id, StudentTripEvent.DroppedOff, studentOnBus.StudentId));
    }

    public async Task CheckAction(string studentHash)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        var student = await childrenManagementService.FindStudentWithHash(studentHash);
        var shuttleSchedule = await scanDeviceSchoolTripService.GetCurrentShuttleScheduleByStudent(student);
        var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == student.Id);

        if (!studentOnBus.IsPickedUp)
            await PickUpStudent(student);
        else if (!studentOnBus.IsDroppedOff && (currentTime - studentOnBus.PickedUpTime) >= TimeSpan.FromMinutes(5))
            await DropOffStudent(student);
    }
}