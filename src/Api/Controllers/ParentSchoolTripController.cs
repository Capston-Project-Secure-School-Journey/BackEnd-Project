using Api.Attributes;
using Api.Common.Enums;
using Api.Services.ParentSchoolTripService;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("parent/school-trips")]
public class ParentSchoolTripController(IParentSchoolTripHandler handler) : ControllerBase
{
    [HttpGet]
    [Authorize(UserType.Parent)]
    public async Task<List<ParentShuttleScheduleResponse>> GetShuttleSchedulesByStudent([FromQuery] Guid studentId,
        [FromQuery] DateOnly date)
    {
        var userId = this.GetUserId();
        return await handler.GetShuttleSchedulesByStudent(userId, studentId, date);
    }
}