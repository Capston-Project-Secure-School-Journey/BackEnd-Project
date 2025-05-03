using Api.Attributes;
using Api.Common.Enums;
using Api.Services.ScheduleManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("schedules")]
public class ScheduleManagementController : ControllerBase
{
    private readonly IScheduleManagementHandler _scheduleManagementHandler;

    public ScheduleManagementController(IScheduleManagementHandler scheduleManagementHandler)
    {
        _scheduleManagementHandler = scheduleManagementHandler;
    }

    [HttpGet("schedule-view")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ClassSchedulePaginationResponse> GetCurrentScheduleView([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await _scheduleManagementHandler.GetScheduleView(schoolId, date);
    }

    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IEnumerable<ClassScheduleResponse>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await _scheduleManagementHandler.CreateSchedule(schoolId, request);
    }

    [HttpPut("{scheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ClassScheduleResponse> UpdateSchedule([FromRoute] Guid scheduleId,
        [FromBody] UpdateScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        request.Id = scheduleId;
        return await _scheduleManagementHandler.UpdateSchedule(schoolId, request);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<List<ClassScheduleResponse>> GetScheduleByDate([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await _scheduleManagementHandler.GetScheduleByDate(schoolId, date);
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteSchedule([FromBody] List<Guid> ids)
    {
        var schoolId = this.GetSchoolId();
        await _scheduleManagementHandler.DeleteSchedule(schoolId, ids);
        return Ok();
    }

    [HttpDelete("{scheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteSchedule([FromRoute] Guid scheduleId)
    {
        var schoolId = this.GetSchoolId();
        await _scheduleManagementHandler.DeleteSchedule(schoolId, scheduleId);

        return Ok();
    }
}