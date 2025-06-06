using Api.Attributes;
using Api.Common.Enums;
using Api.Domain.Models;
using Api.Services.ShuttleScheduleManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("shuttle-schedules")]
public class ShuttleScheduleManagementController(IShuttleScheduleManagementHandler shuttleScheduleManagementHandler)
    : ControllerBase
{
    [HttpGet("pickup-schedule-view")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ShuttleScheduleView> GetShuttleScheduleView([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await shuttleScheduleManagementHandler.GetShuttleScheduleView(schoolId, date);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<Pagination<ShuttleScheduleResponse>> GetShuttleScheduleByDate(
        [FromQuery] GetShuttleScheduleByDateRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await shuttleScheduleManagementHandler.GetShuttleScheduleByDate(schoolId, request);
    }

    [HttpGet("{shuttleScheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ShuttleSchedule> GetShuttleSchedule([FromRoute] Guid shuttleScheduleId)
    {
        var schoolId = this.GetSchoolId();
        return await shuttleScheduleManagementHandler.GetShuttleSchedule(schoolId, shuttleScheduleId);
    }
}