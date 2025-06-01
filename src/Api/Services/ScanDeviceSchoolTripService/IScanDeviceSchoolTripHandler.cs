namespace Api.Services.ScanDeviceSchoolTripService;

public interface IScanDeviceSchoolTripHandler
{
    Task CheckAction(string studentHash);
}