using Api.Services.ScanDeviceSchoolTripService;
using Api.TransferDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("scan-device/school-trips")]
public class ScanDeviceSchoolTripController(IScanDeviceSchoolTripHandler handler): ControllerBase
{
    [HttpPut("pick-up")]
    public async Task PickUpStudent([FromBody] PickUpStudentRequest request)
    {
        await handler.PickUpStudent(request.SecretCode);
    }
    
    [HttpPut("drop-off")]
    public async Task DropOffStudent([FromBody] DropOffStudentRequest request)
    {
        await handler.DropOffStudent(request.SecretCode);
    }
}