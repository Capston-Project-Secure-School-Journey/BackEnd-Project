using Api.Services.ScanDeviceSchoolTripService;
using Api.TransferDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("scan-device/school-trips")]
public class ScanDeviceSchoolTripController(IScanDeviceSchoolTripHandler handler) : ControllerBase
{
    [HttpPut("check-action")]
    public async Task CheckAction([FromBody] CheckActionRequest request)
    {
        await handler.CheckAction(request.SecretCode);
    }
}