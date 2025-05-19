using Api.Attributes;
using Api.Common.Enums;
using Api.Domain.Models;
using Api.Services.DriverSchoolTripService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("driver/school-trips")]
public class DriverSchoolTripController(IDriverSchoolTripHandler handler) : ControllerBase
{
    [HttpGet]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate([FromQuery] DateOnly date)
    {
        var userId = this.GetUserId();
        return await handler.GetShuttleScheduleByDate(userId, date);
    }

    [HttpGet("{shuttleScheduleId}")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ShuttleSchedule> GetShuttleSchedule([FromRoute] Guid shuttleScheduleId)
    {
        var userId = this.GetUserId();
        return await handler.GetShuttleSchedule(shuttleScheduleId, userId);
    }

    [HttpPut("{shuttleScheduleId}/start-journey")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task StartJourney([FromRoute] Guid shuttleScheduleId)
    {
        var userId = this.GetUserId();
        await handler.StartJourney(shuttleScheduleId, userId);
    }

    [HttpPut("{shuttleScheduleId}/end-journey")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task EndJourney([FromRoute] Guid shuttleScheduleId)
    {
        var userId = this.GetUserId();
        await handler.EndJourney(shuttleScheduleId, userId);
    }

    [HttpPut("{shuttleScheduleId}/cancel-journey")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task CancelJourney([FromRoute] Guid shuttleScheduleId, [FromBody] CancelJourneyRequest request)
    {
        var userId = this.GetUserId();
        await handler.CancelJourney(shuttleScheduleId, userId, request.Reason);
    }

    [HttpPut("{shuttleScheduleId}/skip-student")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task SkipStudent([FromRoute] Guid shuttleScheduleId, [FromBody] SkipStudentRequest request)
    {
        var userId = this.GetUserId();
        await handler.SkipStudent(shuttleScheduleId, userId, request.StudentId, request.Reason);
    }

    [HttpGet("has-in-progress-shuttle")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<bool> HasInProgressShuttle()
    {
        var userId = this.GetUserId();
        return await handler.HasInProgressShuttle(userId);
    }

    [HttpGet("current-shuttle")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver()
    {
        var userId = this.GetUserId();
        return await handler.GetCurrentShuttleScheduleByDriver(userId);
    }

    [HttpGet("has-up-coming-shuttle")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<bool> HasUpcomingShuttle()
    {
        var userId = this.GetUserId();
        return await handler.HasUpcomingShuttle(userId);
    }

    [HttpGet("up-coming-shuttle")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<bool> GetUpcomingShuttleSchedule()
    {
        var userId = this.GetUserId();
        return await handler.HasUpcomingShuttle(userId);
    }

    [HttpPut("{shuttleScheduleId}/update-current-location")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task UpdateCurrentLocation([FromRoute] Guid shuttleScheduleId,
        [FromBody] UpdateCurrentLocationRequest request)
    {
        var userId = this.GetUserId();
        await handler.UpdateCurrentAddress(shuttleScheduleId, userId, request.Latitude, request.Longitude);
    }
}