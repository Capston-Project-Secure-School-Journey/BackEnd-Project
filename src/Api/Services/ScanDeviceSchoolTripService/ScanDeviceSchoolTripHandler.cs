namespace Api.Services.ScanDeviceSchoolTripService;

public class ScanDeviceSchoolTripHandler(IScanDeviceSchoolTripService scanDeviceSchoolTripService): IScanDeviceSchoolTripHandler
{
    public async Task PickUpStudent(string studentHash)
    {
        await scanDeviceSchoolTripService.PickUpStudent(studentHash);
    }

    public async Task DropOffStudent(string studentHash)
    {
        await scanDeviceSchoolTripService.DropOffStudent(studentHash);
    }
}