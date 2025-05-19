namespace Api.Services.ScanDeviceSchoolTripService;

public interface IScanDeviceSchoolTripService
{
    Task PickUpStudent(string studentHash);
    Task DropOffStudent(string studentHash);
}