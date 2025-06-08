using Api.Attributes;
using Api.Common.Enums;
using Api.Services.ScheduleManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("schedules")]
public class ScheduleManagementController(IScheduleManagementHandler scheduleManagementHandler) : ControllerBase
{
    [HttpGet("schedule-view")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ClassSchedulePaginationResponse> GetCurrentScheduleView([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await scheduleManagementHandler.GetScheduleView(schoolId, date);
    }

    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IEnumerable<ClassScheduleResponse>> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await scheduleManagementHandler.CreateSchedule(schoolId, request);
    }

    [HttpPut("{scheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ClassScheduleResponse> UpdateSchedule([FromRoute] Guid scheduleId,
        [FromBody] UpdateScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        request.Id = scheduleId;
        return await scheduleManagementHandler.UpdateSchedule(schoolId, request);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<Pagination<ClassScheduleResponse>> GetScheduleByDate([FromQuery] GetScheduleByDateRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await scheduleManagementHandler.GetScheduleByDate(schoolId, request);
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IActionResult> DeleteSchedule([FromBody] List<Guid> ids)
    {
        var schoolId = this.GetSchoolId();
        await scheduleManagementHandler.DeleteSchedule(schoolId, ids);
        return Ok();
    }

    [HttpDelete("{scheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteSchedule([FromRoute] Guid scheduleId)
    {
        var schoolId = this.GetSchoolId();
        await scheduleManagementHandler.DeleteSchedule(schoolId, scheduleId);

        return Ok();
    }

    [HttpPost("clone-week-schedule")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IActionResult> CloneWeekSchedule([FromBody] CloneWeekScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        await scheduleManagementHandler.CloneWeekSchedule(schoolId, request.WeekSource, request.WeekDestination);

        return Ok();
    }

    [HttpPost("clone-day-schedule")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IActionResult> CloneDaySchedule([FromBody] CloneDayScheduleRequest request)
    {
        var schoolId = this.GetSchoolId();
        await scheduleManagementHandler.CloneDaySchedule(schoolId, request.DateSource, request.DateDestination);

        return Ok();
    }
}