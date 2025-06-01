using Api.Domain.Models;

namespace Api.Services.ScanDeviceSchoolTripService;

public interface IScanDeviceSchoolTripService
{
    Task<(ShuttleSchedule, StudentOnBus)> PickUpStudent(Student student);
    Task<(ShuttleSchedule, StudentOnBus)> DropOffStudent(Student student);
    Task<ShuttleSchedule> GetCurrentShuttleScheduleByStudent(Student student);
}