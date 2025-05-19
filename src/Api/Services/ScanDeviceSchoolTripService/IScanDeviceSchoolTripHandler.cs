namespace Api.Services.ScanDeviceSchoolTripService;

public interface IScanDeviceSchoolTripHandler
{
    Task PickUpStudent(string studentHash);
    Task DropOffStudent(string studentHash);
}