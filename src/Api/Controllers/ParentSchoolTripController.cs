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

    [HttpGet("has-in-progress-shuttle")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<bool> HasInProgressShuttle()
    {
        var userId = this.GetUserId();
        return await handler.HasInProgressShuttle(userId);
    }

    [HttpGet("current-shuttle")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<List<ParentShuttleScheduleResponse>> GetCurrentShuttleSchedule()
    {
        var userId = this.GetUserId();
        return await handler.GetCurrentShuttleSchedule(userId);
    }

    [HttpGet("has-up-coming-shuttle")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<bool> HasUpcomingShuttle()
    {
        var userId = this.GetUserId();
        return await handler.HasUpcomingShuttle(userId);
    }

    [HttpGet("up-coming-shuttle")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<List<ParentShuttleScheduleResponse>> GetUpcomingShuttleSchedule()
    {
        var userId = this.GetUserId();
        return await handler.GetUpcomingShuttleSchedule(userId);
    }
}