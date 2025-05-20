using Api.Common.Enums;
using Api.Jobs.SchoolTripEventDispatchJobs;
using Hangfire;

namespace Api.Services.ScanDeviceSchoolTripService;

public class ScanDeviceSchoolTripHandler(IScanDeviceSchoolTripService scanDeviceSchoolTripService)
    : IScanDeviceSchoolTripHandler
{
    public async Task PickUpStudent(string studentHash)
    {
        var (shuttleSchedule, studentOnBus) = await scanDeviceSchoolTripService.PickUpStudent(studentHash);
        BackgroundJob.Enqueue<SendStudentTripEventJob>(
            (job) => job.ExecuteAsync(shuttleSchedule.Id, StudentTripEvent.PickedUp, studentOnBus.StudentId));
    }

    public async Task DropOffStudent(string studentHash)
    {
        var (shuttleSchedule, studentOnBus) = await scanDeviceSchoolTripService.DropOffStudent(studentHash);
        BackgroundJob.Enqueue<SendStudentTripEventJob>(
            (job) => job.ExecuteAsync(shuttleSchedule.Id, StudentTripEvent.DroppedOff, studentOnBus.StudentId));
    }
}