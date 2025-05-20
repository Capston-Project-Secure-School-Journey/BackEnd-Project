using Api.Domain.Models;

namespace Api.Services.ScanDeviceSchoolTripService;

public interface IScanDeviceSchoolTripService
{
    Task<(ShuttleSchedule, StudentOnBus)> PickUpStudent(string studentHash);
    Task<(ShuttleSchedule, StudentOnBus)> DropOffStudent(string studentHash);
}